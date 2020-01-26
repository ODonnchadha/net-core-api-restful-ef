using Api.EF;
using Api.Helpers;
using Api.Interfaces;
using Api.Repositories;
using Api.Services;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NLog.Extensions.Logging;
using System.Linq;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LibraryDbContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<AuthorsResourceParameters, AuthorsResourceParameters>();

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<ITypeHelperService, TypeHelperService>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<ILibraryRepository, LibraryRepository>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            ConfigureAutoMapper();

            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                var xmlDataContractSerializerInputFormatter = new XmlDataContractSerializerInputFormatter(setupAction);
                xmlDataContractSerializerInputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.authorwithdateofdeath.full+xml");
                setupAction.InputFormatters.Add(xmlDataContractSerializerInputFormatter);

                var jsonInputFormatter = setupAction.InputFormatters.OfType<JsonInputFormatter>().FirstOrDefault();
                if (jsonInputFormatter != null)
                {
                    jsonInputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.author.full+json");
                    jsonInputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.authorwithdateofdeath.full+json");
                }

                var jsonOutputFormatter = setupAction.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                if (jsonOutputFormatter != null)
                {
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
                }
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddHttpCacheHeaders(
                expirationModelOptionsAction => { expirationModelOptionsAction.MaxAge = 600; },
                validationModelOptionsAction => { validationModelOptionsAction.MustRevalidate = true; });

            services.AddResponseCaching();

            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>((options) => 
            {
                options.GeneralRules = new System.Collections.Generic.List<RateLimitRule>()
                {
                    new RateLimitRule { Endpoint = "*", Limit = 10, Period = "5m" },
                    new RateLimitRule { Endpoint = "*", Limit = 2, Period = "10s" }
                };
            });

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        }

        public void ConfigureAutoMapper()
        {
            AutoMapper.Mapper.Initialize(cfg => {
                cfg.CreateMap<Models.Author, DTOs.ReadAuthor>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(
                        src => $"{src.FirstName} {src.LastName}"))
                //.ForMember(
                //    dest => dest.Age,
                //    opt => opt.MapFrom(
                //        src => src.DateOfBirth.GetCurrentAge()));
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(
                        src => src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));
                cfg.CreateMap<Models.Book, DTOs.ReadBook>();
                cfg.CreateMap<Models.Book, DTOs.UpdateBook>();

                cfg.CreateMap<DTOs.CreateAuthor, Models.Author>();
                cfg.CreateMap<DTOs.CreateAuthorWithDateOfDeath, Models.Author>();
                cfg.CreateMap<DTOs.CreateBook, Models.Book>();
                cfg.CreateMap<DTOs.UpdateBook, Models.Book>();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            LoggerFactory.AddConsole();
            LoggerFactory.AddDebug(LogLevel.Information);

            LoggerFactory.AddNLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (null != exceptionHandlerFeature)
                        {
                            var logger = LoggerFactory.CreateLogger("Global Exception Logger");
                            logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                        }

                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault did occur. Please try again later.");
                    });
                });
            }

            app.UseIpRateLimiting();
            app.UseResponseCaching();
            app.UseHttpCacheHeaders();
            app.UseMvc();
        }
    }
}
