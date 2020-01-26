using Api.DTOs;
using Api.Interfaces;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Controllers
{
    [ApiController, Route("api/authors/{authorId}/books")]
    public class BooksController : ControllerBase
    {
        private readonly ILibraryRepository repository;
        private readonly ILogger<BooksController> logger;
        private readonly IUrlHelper urlHelper;

        public BooksController(ILibraryRepository repository, ILogger<BooksController> logger, IUrlHelper urlHelper)
        {
            this.repository = repository;
            this.logger = logger;
            this.urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetBooksForAuthor")]
        public IActionResult Get(Guid authorId)
        {
            if (!repository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var books = repository.GetBooksForAuthor(authorId);

            var readBooks = Mapper.Map<IEnumerable<ReadBook>>(books);

            readBooks = readBooks.Select(b =>
            {
                b = CreateLinksForBook(b);
                return b;
            });

            var wrapper = new LinkedCollectionResourceWrapper<ReadBook>(readBooks);
            return Ok(CreateLinksForBooks(wrapper));
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult Get(Guid authorId, Guid id)
        {
            if (!repository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = repository.GetBookForAuthor(authorId, id);
            if (null == book)
            {
                return NotFound();
            }

            var readBook = Mapper.Map<ReadBook>(book);

            return Ok(CreateLinksForBook(readBook));
        }

        [HttpPost("{id}", Name ="CreateBookForAuthor")]
        public IActionResult Post(Guid authorId, [FromBody]CreateBook createBook)
        {
            if (null == createBook)
            {
                return BadRequest();
            }

            if (createBook.Description == createBook.Title)
            {
                ModelState.AddModelError(nameof(createBook), "Provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!repository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = Mapper.Map<Models.Book>(createBook);

            repository.AddBookForAuthor(authorId, book);

            if (!repository.Save())
            {
                throw new Exception($"Book create for author { authorId } failed.");
            }

            var readBook = Mapper.Map<ReadBook>(book);

            return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = readBook.Id }, CreateLinksForBook(readBook));
        }

        [HttpDelete("{id}", Name = "DeleteBookForAuthor")]
        public IActionResult Delete(Guid authorId, Guid id)
        {
            if (!repository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = repository.GetBookForAuthor(authorId, id);
            if (null == book)
            {
                return NotFound();
            }

            repository.DeleteBook(book);
            if (!repository.Save())
            {
                throw new Exception($"Delete book { id } for author { authorId } failed.");
            }

            logger.LogInformation(100, $"Book { id } for author { authorId } was deleted.");
            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateBookForAuthor")]
        public IActionResult Put(Guid authorId, Guid id, [FromBody]UpdateBook updateBook)
        {
            if (null == updateBook)
            {
                return BadRequest();
            }

            if (updateBook.Description == updateBook.Title)
            {
                ModelState.AddModelError(nameof(updateBook), "Provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!repository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = repository.GetBookForAuthor(authorId, id);
            if (null == book)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;

                repository.AddBookForAuthor(authorId, bookToAdd);

                if (!repository.Save())
                {
                    throw new Exception($"Upserting book { id } for author { authorId } failed.");
                }

                var readBook = Mapper.Map<ReadBook>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = readBook.Id }, readBook);
            }

            Mapper.Map(updateBook, book);
            repository.UpdateBookForAuthor(book);

            if (!repository.Save())
            {
                throw new Exception($"Update book { id } for author { authorId } failed.");
            }

            return NoContent();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateBookForAuthor")]
        public IActionResult Patch(Guid authorId, Guid id, [FromBody]JsonPatchDocument<UpdateBook> patchDoc)
        {
            if (null == patchDoc)
            {
                return BadRequest();
            }

            if (!repository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = repository.GetBookForAuthor(authorId, id);
            if (null == book)
            {
                var updateBook = new UpdateBook();
                patchDoc.ApplyTo(updateBook, ModelState);

                if (updateBook.Description == updateBook.Title)
                {
                    ModelState.AddModelError(nameof(updateBook), "The provided description should be different from the title.");
                }
                TryValidateModel(updateBook);

                if (!ModelState.IsValid)
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var bookToAdd = Mapper.Map<Book>(updateBook);
                bookToAdd.Id = id;

                repository.AddBookForAuthor(authorId, bookToAdd);

                if (!repository.Save())
                {
                    throw new Exception($"Upsert book { id } for author { authorId } failed.");
                }

                var bookToReturn = Mapper.Map<ReadBook>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
            }

            var bookToPatch = Mapper.Map<UpdateBook>(book);
            patchDoc.ApplyTo(bookToPatch);

            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(bookToPatch), "The provided description should be different from the title.");
            }
            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            Mapper.Map(bookToPatch, book);
            repository.UpdateBookForAuthor(book);

            if (!repository.Save())
            {
                throw new Exception($"Patch book { id } for author { authorId } failed.");
            }

            return NoContent();
        }

        private ReadBook CreateLinksForBook(ReadBook book)
        {
            book.Links.Add(new Link(urlHelper.Link("GetBookForAuthor", new { id = book.Id }), "self", "GET"));
            book.Links.Add(new Link(urlHelper.Link("DeleteBookForAuthor", new { id = book.Id }), "delete_book", "DELETE"));
            book.Links.Add(new Link(urlHelper.Link("UpdateBookForAuthor", new { id = book.Id }), "update_book", "PUT"));
            book.Links.Add(new Link(urlHelper.Link("PartiallyUpdateBookForAuthor", new { id = book.Id }), "partially_update_book", "PATCH"));

            return book;
        }

        private LinkedCollectionResourceWrapper<ReadBook> CreateLinksForBooks(LinkedCollectionResourceWrapper<ReadBook> wrapper)
        {
            wrapper.Links.Add(new Link(urlHelper.Link("GetBooksForAuthor", new { }), "self", "GET"));
            return wrapper;
        }
    }
}