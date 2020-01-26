using Api.DTOs;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ILibraryRepository repository;
        private readonly IPropertyMappingService propertyMappingService;
        private readonly ITypeHelperService typeHelperService;
        private readonly IUrlHelper urlHelper;

        public AuthorsController(
            ILibraryRepository repository, 
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService,
            IUrlHelper urlHelper)
        {
            this.repository = repository;
            this.propertyMappingService = propertyMappingService;
            this.typeHelperService = typeHelperService;
            this.urlHelper = urlHelper;
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult Get(Guid id, [FromQuery]string fields)
        {
            if (!typeHelperService.TypeHasProperties<ReadAuthor>(fields))
            {
                return BadRequest();
            }

            var author = repository.GetAuthor(id);
            if (author == null)
            {
                return NotFound();
            }

            var readAuthor = Mapper.Map<ReadAuthor>(author);

            var links = CreateLinksForAuthor(id, fields);
            var linkedResourceToReturn = readAuthor.ShapeData(fields) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            // return Ok(readAuthor.ShapeData(fields));
            return Ok(linkedResourceToReturn);
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead()]
        public IActionResult Get([FromQuery]AuthorsResourceParameters authorsResourceParameters, [FromHeader(Name ="Accept")] string mediaType)
        {
            if (!propertyMappingService.ValidMappingExistsFor<ReadAuthor, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!typeHelperService.TypeHasProperties<ReadAuthor>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            var authors = repository.GetAuthors(authorsResourceParameters);

            var readAuthors = Mapper.Map<IEnumerable<ReadAuthor>>(authors);

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(new
                {
                    totalCount = authors.TotalCount,
                    pageSize = authors.PageSize,
                    currentPage = authors.CurrentPage,
                    totalPages = authors.TotalPages
                }));

                var links = CreateLinksForAuthors(authorsResourceParameters, authors.HasNext, authors.HasPrevious);
                var shapedAuthors = readAuthors.ShapeData(authorsResourceParameters.Fields);

                var shapedAuthorsWithLinks = shapedAuthors.Select(a =>
                {
                    var authorAsDictionary = a as IDictionary<string, object>;

                    var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["id"], authorsResourceParameters.Fields);
                    authorAsDictionary.Add("links", authorLinks);

                    return authorAsDictionary;
                });

                var linkedCollectionResource = new { value = shapedAuthorsWithLinks, links = links };

                return Ok(linkedCollectionResource);
            }
            else
            {
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(new
                {
                    totalCount = authors.TotalCount,
                    pageSize = authors.PageSize,
                    currentPage = authors.CurrentPage,
                    totalPages = authors.TotalPages,
                    previousPageLink = authors.HasPrevious ?
                        CreateAuthorsResourceUri(
                        authorsResourceParameters, ResourceUriType.PreviousPage) :
                        null,
                    nextPageLink = authors.HasNext ?
                        CreateAuthorsResourceUri(
                        authorsResourceParameters, ResourceUriType.NextPage) :
                        null
                }));

                return Ok(readAuthors.ShapeData(authorsResourceParameters.Fields));
            }
        }

        [HttpPost(Name = "CreateAuthor")]
        [RequestHeaderMatchesMediaType("Content-type", new[] { "application/vnd.marvin.author.full+json" })]
        public IActionResult Post([FromBody]CreateAuthor createAuthor)
        {
            if (null == createAuthor)
            {
                return BadRequest();
            }

            var author = Mapper.Map<Author>(createAuthor);

            repository.AddAuthor(author);

            if (!repository.Save())
            {
                throw new Exception("Author create failed.");
            }

            var readAuthor = Mapper.Map<ReadAuthor>(author);

            var links = CreateLinksForAuthor(readAuthor.id, null);

            var linkedResourceToReturn = readAuthor.ShapeData(null) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor", new { id = linkedResourceToReturn["id"] }, linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
        [RequestHeaderMatchesMediaType("Content-type", new[] {
            "application/vnd.marvin.authorwithdateofdeath.full+json", "application/vnd.marvin.authorwithdateofdeath.full+xml" })]
        public IActionResult Post([FromBody]CreateAuthorWithDateOfDeath createAuthorWithDateOfDeath)
        {
            if (null == createAuthorWithDateOfDeath)
            {
                return BadRequest();
            }

            var author = Mapper.Map<Author>(createAuthorWithDateOfDeath);

            repository.AddAuthor(author);

            if (!repository.Save())
            {
                throw new Exception("Author create failed.");
            }

            var readAuthor = Mapper.Map<ReadAuthor>(author);

            var links = CreateLinksForAuthor(readAuthor.id, null);

            var linkedResourceToReturn = readAuthor.ShapeData(null) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor", new { id = linkedResourceToReturn["id"] }, linkedResourceToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult Post(Guid id)
        {
            if (repository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}", Name ="DeleteAuthor")]
        public IActionResult Delete(Guid id)
        {
            var author = repository.GetAuthor(id);
            if (author == null)
            {
                return NotFound();
            }

            repository.DeleteAuthor(author);

            if (!repository.Save())
            {
                throw new Exception("Author delete failed.");
            }

            return NoContent();
        }

        [HttpOptions()]
        public IActionResult Get()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters parameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = parameters.Fields,
                            orderBy = parameters.OrderBy,
                            searchQuery = parameters.SearchQuery,
                            genre = parameters.Genre,
                            pageNumber = parameters.PageNumber - 1,
                            pageSize = parameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = parameters.Fields,
                            orderBy = parameters.OrderBy,
                            searchQuery = parameters.SearchQuery,
                            genre = parameters.Genre,
                            pageNumber = parameters.PageNumber + 1,
                            pageSize = parameters.PageSize
                        });
                case ResourceUriType.Current:
                default:
                    return urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = parameters.Fields,
                            orderBy = parameters.OrderBy,
                            searchQuery = parameters.SearchQuery,
                            genre = parameters.Genre,
                            pageNumber = parameters.PageNumber,
                            pageSize = parameters.PageSize
                        });
            }
        }

        private IEnumerable<Link> CreateLinksForAuthor(Guid id, string fields)
        {
            var links = new List<Link>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new Link(urlHelper.Link("GetAuthor", new { id = id }), "self", "GET"));
            }
            else
            {
                links.Add(new Link(urlHelper.Link("GetAuthor", new { id = id, fields = fields }), "self", "GET"));
            }

            links.Add(new Link(urlHelper.Link("DeleteAuthor", new { id = id }), "delete_author", "DELETE"));
            links.Add(new Link(urlHelper.Link("CreateBookForAuthor", new { authorId = id }), "create_book_for_author", "POST"));

            return links;
        }

        private IEnumerable<Link> CreateLinksForAuthors(AuthorsResourceParameters parameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<Link>();

            links.Add(new Link(CreateAuthorsResourceUri(parameters, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(new Link(CreateAuthorsResourceUri(parameters, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new Link(CreateAuthorsResourceUri(parameters, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }
    }
}
