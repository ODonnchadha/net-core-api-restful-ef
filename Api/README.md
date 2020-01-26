## net-core-api-restful-ef
- .NET Core 2.1 Web API using *best* RESTful practices. e.g.: Level 3 REST and support and the PATCH verb.
- AutoMapper, AspNetCoreRateLimit, MarvinCacheHeaders, NLog, and System.Linq.Dynamic.Core are heavily embraced.
- See NOTES.MD for any related credit or blame.

## Run
- We need to setup and seed to localdb. Open the Package Manager Console and type the following:
- PM> add-migration RESTfulLibrary
- PM> update-database
- Compile and launch the application.
- I use Microsoft SQL Server Management Studio with Server name: (localdb)\MSSQLLocalDB.

```csharp
services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

{
   "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RESTfulLibrary;Trusted_Connection=false;MultipleActiveResultSets=true" 
   }
}
```