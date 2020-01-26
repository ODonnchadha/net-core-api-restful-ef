# Introduction:
- Culled from the PluralSight class: Building a RESTful API with ASP.NET Core
	- Kevin Dockx. 8h 10m Mar 13, 2017

# Start Me Up:
- e.g.: services.AddTransient<ILibraryRepository, LibraryRepository>();
	- Transient objects are always different; a new instance is provided to every controller and every service.
	- Scoped objects are the same within a request, but different across different requests.
	- Singleton objects are the same for every object and every request.

# Introducing REST:
- Representational State Transfer is defined by six (6) constraints:

- 1. CLIENT/SERVER: Client and server are separate are can evolve separately.
- 2. STATELESSNESS: State is contained within the request.
- 3. CACHEABLE: Each rsponse message must explicitly state if it can be cached or not.
- 4. LAYERED SYSTEM: Client cannot discern as to what layer it's connected to.
- 5. CODE ON DEMAND (OPTIONAL): Server can extend client functionality.
- 6. UNIFORM INTERFACE: Consumers share one single, technical interface: URI, Method, Media Type.
  -  a. Identification of resources: A resource is conceptually separate from its representation.
  -  b. Manupilation of resources through representations: Representation and metadata should be sufficient to modify or delete the resource.
  -  c. Self-descriptive message: Each message must include enough information on how to process *that* message.
  -  d. HATEAOS: Hypermedia as the engine of application state.

# The Richardson Maturity Model:
- 1. Level 0 (The Swamp of POX:) HTTP protocol is used for remote interaction.
- 2. Level 1 (Resources:) Multiple URIs. Each mapped to a separate resource.
- 3. Level 2 (Verbs:) GET, POST, DELETE, PUT with correct status codes.
- 4. Level 3 (Hypermedia:) HATEAOS: HTTP status plus discoverability - links that drive application state.

# Starter Solution:
- KevinDockx/RESTfulAPIAspNetCore_Course
- Building a RESTful API with ASP.NET Core starter files

# Resourse Naming Guidlines:
 - Nouns: Things, not actions. e.g.: api/authors api/authors/{id} api/authors/{id}/books
    - NOTE: Filter and sort are not resources and, thus, should be passed via query string and not as resources.
    - NOTE: A pluralized resource should be followed by an id.
- Resource URIs should remain the same. e.g.: Should not map to backend identity column. 
    - NOTE: REST stops at the outer-facing contract.
    - NOTE: GUIDs can be used and may hide implementation details.

# Routing:
- Convention-based routing can be inserted here:	app.UseMvc();
- Attribute-based routing (as such:)				[Route("api/[controller]")]

# Resources:
- PATCH:	JsonPatchDocument
- HEAD:		Identical to GET, but API should not return a response body. No payload. We obtain information about the resource, such as "Do you exist?"
- OPTIONS:	A request for information about communication options available on the URI.

# Outer-Facing Model vs. Entity Model:
 - DTOs vs. Models

# The Importance Of Being Earnest (With Status Codes:)
- Part of the HTTP standard. What is responsible for a failed request?
- Five Levels:
	1. LEVEL 100: Not used by APIs
	2. LEVEL 200: The request went well
		a. 200: OK
		b. 201: Created
		c. 204: No Content
	3. LEVEL 300: Redirection
	4. LEVEL 400: Client mistakes
		a. 400: Bad request
		b. 401: Unauthorized
		c. 403: Forbidden. Authentication succeeded, but user does not have access.
		d. 404: Not Found
		e. 405: Method not allowed. e.g.: POST request when only GET is implemented on *that* resource.
		f. 406: Not acceptable. e.g.: Request was for XML, but only JSON is supported.
		g. 409: Conflict. e.g.: POST of a resource that already exists.
		h. 415: Unsupported media type
		i. 422: Unprocessable entity
	4. LEVEL 500: Server mistakes
		a. 500: Internal server error

# 500 Exceptions:
	a. try{}.. catch(Exception e) { return StatusCode(500, "Error message."); throw e; }
	b. Global exception handling:
		app.UseExceptionHandler(appBuilder =>
		{
			appBuilder.Run(async context =>
			{
				context.Response.StatusCode = 500;
				await context.Response.WriteAsync("An unexpected fault did occur. Please try again later.");
			});
		});

# Formatters and Content Negotation:
- Key concept: Select the best representation for a given response when there are multiple representations available.
- Media type is passed via the ACCEPT header request: e.g.: application/json, application/xml.
- Return 406: Not acceptable. Best practice is to include an accept header.
- Output formatter: accept header. Input formatter: content-type header.

- GET:	Accept: application/xml

# Method safety and Method Idempotency
 - A method is considered *safe* when it doesn't change the resource representation.
 - A method is considered idempotent when it can be called multiple times with the same result.

	 HTTP METHOD		SAFE?		IDEMPOTENT?
	 GET				Yes			Yes
	 OPTIONS			Yes			Yes
	 HEAD				Yes			Yes
	 POST				No			No
	 DELETE				No			Yes
	 PUT				No			Yes
	 PATCH				No			No

# [FromBody] 
- Deserialized from the request body.
- return CreatedAtRoute("GetAuthor", new { id = dto.id }, dto);

Content-Type: application/json
Accept: application/json
{ firstName: "Flann", lastName: "O'Brien", DateOfBirth: "1966-04-01T17:00:00", genre: "Fiction" }

Location header: e.g.: http://localhost:17791/api/Authors/61b2b15d-d5b6-4218-800f-4eaa4abbfd1c

# Creating a Collection of Resources:
- e.g.: Creating a collection of authors

# Working with array keys and composite keys:
- Array key. e.g.: comma-seperated list: (key1,key2,key3)
- a.) IEnumerable<Guid> ids: No implicit binding. We'll create our own.

- Composite keys: key/value pairs.

# Should never POST to a single resource
- e.g.: /api/authors/{guid}
- The server creates the URI, not the client. POST is not idempotent.

- 404 returned for free on route that does not exist.
- With POST to existing resource, we want a 409.
- e.g.: return new StatusCodeResult(StatusCodes.Status409Conflict);

# If we don't supply a Content-Type header when providing a request body:
- 415 Unsupported Media Type
- e.g.: Content-Type: application/xml
- e.g.: Accept: application/xml

```javascript
	services.AddMvc(setupAction =>
	{
		setupAction.ReturnHttpNotAcceptable = true;
		setupAction.InputFormatters.Add(item: new XmlDataContractSerializerInputFormatter(setupAction));
		setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
	}).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
```

# Deleting a resource:
- return NoContent(). 
- 204 Not Found. Idempotent.
- Deleting a parent resource can have an effect on its children resource(s). Cascade DELETE is in effect.
- NOTE: Sending a DELETE request to a collection resource is allowed, yet it's very rarely implemented because it can be very destructive.
- This is different than deleteing an author's books due to an author delete.

# SUMMARY:
- A method is considered safe when it doesn't change the resource representation.
- A method is considered idempotent when it can be called multiple times with the same result.

- POST is used to create resources: Unsafe. Not idempotent. 201 Created. Location header.
- Use the content-type header to signify the media type of the request body.

- DELETE is used to delete resources: Unsafe. Idempotent. 204 No Content.

# The Repository Pattern
- An abstraction that reduces complexity and aims to make the code, safe for the repository implementation, persistence ignorant.
- Advantages:
	1. Less code duplication
	2. Less error-prone code
	3. Better testability of the consuming class
- Persistence ignorant: Switching out the persistence technology is not the mail purpose. Chosing the best one for each repository is.

- We're working on a contract, not on an oimplementation
- Always have a set of methods matching the required functionality and call them, even if they don't do anything in the current implementation.

- In EF Core, entities are tracked by the context. 
- By executing: Mapper.Map() the entity has changed to a modified state and executing a 'save' will write changes to the database.

# Updating a resource, including "upserting."
- PUT for full updates. 200 OK or 204 No Content. Not safe. Idempotent.
- Sending a PUT request to a collection resource is allowed: e.g.: api/authors/{authorId}/books
- The resouce would be overridden with the new collection. This is allowed but not advised.

- UPSERTING: Creating a resource with PUT or PATCH. Possible when the consumer of the API is allowed to create the resource URI.

- PATCH for partial updates. Pass a change set to a resource. Not safe. Idempotent.
- JSON Patch standard: Media type: "application/json-patch+json"
- e.g.:
```javascript
	{
		"op": "replace",
		"path": "/title",
		"value": "new title"
	},
	{
		"op": "remove",
		"path": "/description"
	}
```
- JSON Patch Operations: Add, Remove, Replace, Copy, Move, Test (is a value at equal to another value?)

# Validation and Logging:
- 1.) Define validation rules:
	- We'll use ModelState:
	- A dictionary containing both the state of the model, and the binding validation.
- 2.) Check validation rules
	- Contains a collection of error messages for each property value submitted. e.g.: ModelState.IsValid();
- 3.) Report validation errors
	- Response status should be 422. Unprocessable entity.

- Validation Errors:
	- Use DataAnnotations on the DTOs (which I hate.) And leverage ModelState within the controller. If error, please return a 422.
- PUT Validation:
	- With DataAnnotations, we end up mixing in rules with models. Also, we added an abstract DTO class for 'book' (which I hate.)
	- Recommended: Fluent validation.
	- TryValidateModel(); Any errors will now end up in the ModelState.
- UPSERT Validation:
	- Similiar to patch document.

Faults: (Level 500)
	- Create a logger via a LoggerFactory.
- Log to file:
	- Add Nuget package: NLog.Web.AspNetCore
	- Core 1.*: Use extension method LoggerFactory.AddNLog() instead of LoggerFactory.AddProvider(new NLog.Extensions.Logging.NLogLoggerProvider());
	- Core 2.*: Chain AddNLog() within CreateWebHostBuilder for immediate logging.

# Paging, Filtering, and Searching:
- Apparently we don't want to return 1000s of authors with a given response. e.g.: authors?pageNumber=1&pageSize=5
- Page size should be limited and you *should* page by default. So we push these values all the way into our data store.

- The principle of deferred: Execution is deferred until the query is iterated over. 
	- 1. IQueryable<T> creates an expression tree
	- 2. Iteration: foreach loop or ToList(), ToArray(), ToDictionary() or Singleton queries (e.g.: Avg(), Sum())

- Paging:
	- e.g.: public IActionResult Get([FromQuery]int pageNumber = 1, int pageSize = 10)
	- Model Binding: public IActionResult Get([FromQuery]AuthorsResourceParameters authorsResourceParameters)
	- Add paging last within the query so that you are leveraging OrderBy() and ThenBy() with your results.
	- e.g.: http://localhost:17791/api/authors?pageNumber=3&pageSize=1

- Pagination Metadata:
	- At the very least, include links to the previous and next pages. Can include total count, amount of pages...
	- Note: With application/json request "metadata" is not part of the resource representation. Use a custom header like "X-Pagination."
	- Create a custom list type that will hold the metadata.

- Filtering:
	- Limits the collection resource, taking into account a predicate.
	- Filter ONLY on fields of the resource, not on fields of lower-level layer objects.
- Searching:
	- Limits the collection resource by passing a search predicate.

# Sorting and Data Shaping:
	- e.g.: ?orderBy=age || ?orderBy=age desc
		- Account for field and order direction
		- Map DTO property names to 'sort' entity names:
			- a. Use the System.Linq.Dynamic.Core library
			- b. Create a property mapping service
	- Taking invalid input into account.

	- Shaping: Allow the consumer of the API to choose the resource fields. So we'll be dynamically creating an object a run-time.
	- e.g.: /api/authors?fields=id,name
	- Note: Beware of losing the associated camel casing with shape data. Expando Object uses a backing dictionary object and its 'Key' is the property name.
	- Additional:
	- (1) Expand child resources: ?expand=books 
	- (2) Shape expanded resources: ?fields=id,name,books.title
	- (3) Complex filters: ?genre=contains('horror')

	- Most APIs do not need all of these options.

# HATEOAS: Hypermedia as the engine of application state. "Worst acronym ever." - Jeff Albertson
- Evolvability and self-descriptiveness. Hypermedia drives how to consume and use the API.
- Without: Intrinsic knowledge of API contract. Additional/changes to rules break consumers of the API.
- e.g.: links could be added to the API: 
```javascript
	{ "links": [{
		"href":"contains the uri to be invoked",
		"rel":"identifies the type of action",
		"method": "defines the method to use"
	  }] 
	}
```
- Controls learned on the fly. Self-generating. The links will be created based upon business rules and then added to the output.
- Two approaches:
	- 1.) Statically typed approach: Base class with links and wrapper class for collection resources.
		- a.) Currenty creating * complex * objects (e.g.: private set, abstract class) within DTOs. I hate this approach.
	- 2.) Dynamically typed approach. Anonymous types and ExpandoObject. Using IEnumerable<ExpandoObject>();

# HATEOAS: Media Types and Versioning
- HATEOAS and content negotation: { "value" [], "links": [] } Metadata goes in the header (Metadata: describing the resource.)
- Our solution is to create a new media type. A vendor-specific media type: application/vnd.company.hateoas+json.
- If this new media type os requested, the "links" should be included.
```javascript
	[FromHeader(Name ="Accept")] string mediaType
```

- From application/json to application/vnd.marvin.author.friendly+json or application/vnd.marvin.author.full+json
- Versioning in a RESTful world: (a) Functionality (b) Business rules (c) Resource representations
- 1. Through the URI. e.g.: api/v1/authors
- 2. Through the query string. e.g.: api/authors?api-version=v1
- 3. Through a custom header. e.g.: "api-version"=v1

- Use HATEOAS to adapt to changes in functionality and business rules.
- Use Col (Code on Demand) to adapt to changes in media types/resource representations.
- Version media types to handle change in representations:
- application/vnd.marvin.author.friendly.v1+json
- application/vnd.marvin.author.friendly.v2+json

-- Add public DateTimeOffset DateOfDeath { get; set; }
```javascript
	PM> add-migration AddDateOfDeathToAuthor
```
- Other Approaches:
	- HAL (Hypertext Application Language)
	- SIREN (Structured Interface for Representing Entities)
	- JSON-LD (Lined Data Format) and JSON-API and OData

# Working with Caching and Concurrency
- Working with caching: Every response should define itself as cacheable or not. Use HTTP Caching.

- Three (3) Cache Types:
	- 1. Client Cache Private cache. Resources are not shared and kept on the client. Mobile device or local storage.
	- 2. Gateway cache. Shared accross different applications. Reverse-proxy cache. Lives on the server.
	- 3. Proxy cache. Lives on the nework and is a shared cache.

- Expiration Model: Expires header vs. Cache-Control header.
- Use validators to check cache: Strong (ETag) versus weak (Last-Modified.)
- Server => 304 Not Modified => Cache 200 OK => Client

- Cache-Control Directives:
	- Response:
	-	(a) Freshness. max-age, s-maxage.
	-	(b) Cache type: public, private.
	-	(c) Validation: no-cache, must-revalidate, proxy-revalidate.
	-	(d) Other: no-store, no-transform.
	- Request:
	-	(a) Freshness. max-age, max-fresh, max-stale.
	-	(b) Validation: no-cache.
	-	(c) Other: no-store, no-transform, only-if-cached.
- Note: POSTMAN configuration: Turn "Send no-cache header" configuration on.

- Concurrency in a RESTful world. Strategies:
- a.) Pessimistic concurrency: Resource is locked and while it's locked cannot be modified by another client. This is not possible in REST.
- b.) Optimistic concurrency: Token is returned together with the resource. As long as token remains valid, an update can occur. ETags are used.
- Note: Etag is returned on GET. PUT is allowed with If-Match header or we receive a 412 Precondition failed if a different Etag is associated with the (since updated) resource.

# Protecting, Testing, and Documentating Our API
- Rate limiting and throttling.
- Too many requests. Scraping robots. e.g.: 100 requests per hour per client.
- Allowed requests: X-Rate-Limit-Limit, X-Rate-Limit-Remaining, X-Rate-Limit-Reset
- Disallowed requests: 429 Too many requests with Retry-After response header.

- Testing our API during development:
- Postman: run a full set of tests, much like unit tests, and validate the result. Postman contains a fully-fledged testing sandbox.

- Use Tests tab to add a 'test.' e.g.:

- tests["Status code is 200"] = responseCode.code === 200;

- var jsonData = JSON.parse(responseBody);
- var idMatches = jsonData.id === "76053df4-6687-4353-8937-b45556748aaa"
- tests["id matches returned id"] = idMatches;

- tests["X-Pagination is present"] = postman.getResponseHeader("X-Pagination");

- Use the collection runner with Import Test Run in order to test collections.

- Automated Testing and Continuous Integration:
- Run tests after successful build. Newman is a command-line collection runner for Postman.
- Add an additional build stet calling into Newman to run a collection of tests.
- >> newman -c mycollection.postman_collection --exitCode 1

- Documentating our API. How to consume. Not a technical design document:
- (Resource identifier) URIs, HTTP methods, and Payload (including media types.)
- How? a. Swagger OpenAPI. b. Swashbuckle (uses ApenAPI v2 with no support for action overloading.)

- OPTIONS and HEAD:
	- OPTIONS: A request for information.
	- Determine the option and/or requirements associated with a resource.
	- Determine the capabilities of a server, wittout implying a resource action or initiating a resource retrieval.
	- e.g.: OPTIONS api/authors 200 OK Allow: GET,OPTIONS,POST

	- HEAD: Identical to GET, but without a response body. All headers associated should be included.