using Api.DTOs;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ILibraryRepository repository;
        public AuthorCollectionsController(ILibraryRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] IEnumerable<CreateAuthor> createAuthors)
        {
            if (null == createAuthors)
            {
                return BadRequest();
            }

            var authors = Mapper.Map<IEnumerable<Author>>(createAuthors);

            foreach(var author in authors)
            {
                repository.AddAuthor(author);
            }

            if (!repository.Save())
            {
                throw new Exception("Creating an author collection failed on save.");
            }

            var readAuthors = Mapper.Map<IEnumerable<ReadAuthor>>(authors);
            var readAuthorIds = string.Join(",", readAuthors.Select(d => d.id));

            return CreatedAtRoute("GetAuthorCollection", new { ids = readAuthorIds }, readAuthors);
        }

        [HttpGet("({ids})", Name ="GetAuthorCollection")]
        public IActionResult Get([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (null == ids)
            {
                return BadRequest();
            }

            var authors = repository.GetAuthors(ids);

            if (ids.Count() != authors.Count())
            {
                return NotFound();
            }

            var readAuthors = Mapper.Map<IEnumerable<ReadAuthor>>(authors);

            return Ok(readAuthors);
        }
    }
}