using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Controllers
{
    [Route("api")]
    public class RootController : Controller
    {
        private readonly IUrlHelper urlHelper;

        public RootController(IUrlHelper urlHelper)
        {
            this.urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetRoot")]
        public IActionResult Get([FromHeader(Name = "Accept")]string mediaType)
        {
            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var links = new List<Link>();

                links.Add(new Link(urlHelper.Link("GetRoot", new { }), "self", "GET"));
                links.Add(new Link(urlHelper.Link("GetAuthors", new { }), "authors", "GET"));
                links.Add(new Link(urlHelper.Link("CreateAuthor", new { }), "create_author", "POST"));

                return Ok(links);
            }

            return NoContent();
        }
    }
}