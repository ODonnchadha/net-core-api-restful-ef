using Api.DTOs;
using Api.EF;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Repositories
{
    public class LibraryRepository : ILibraryRepository
    {
        private LibraryDbContext context;
        private IPropertyMappingService propertyMappingService;

        public LibraryRepository(LibraryDbContext context, IPropertyMappingService propertyMappingService)
        {
            this.context = context;
            this.propertyMappingService = propertyMappingService;
        }

        public void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();
            context.Authors.Add(author);

            if (author.Books.Any())
            {
                foreach (var book in author.Books)
                {
                    book.Id = Guid.NewGuid();
                }
            }
        }

        public void AddBookForAuthor(Guid authorId, Book book)
        {
            var author = GetAuthor(authorId);
            if (author != null)
            {
                if (book.Id == Guid.Empty)
                {
                    book.Id = Guid.NewGuid();
                }
                author.Books.Add(book);
            }
        }

        public bool AuthorExists(Guid authorId)
        {
            return context.Authors.Any(
                a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            context.Authors.Remove(author);
        }

        public void DeleteBook(Book book)
        {
            context.Books.Remove(book);
        }

        public Author GetAuthor(Guid authorId)
        {
            return context.Authors.FirstOrDefault(
                a => a.Id == authorId);
        }

        public PagedList<Author> GetAuthors(AuthorsResourceParameters pagination)
        {
            //var callectionBeforePaging = context.Authors
            //    .OrderBy(a => a.FirstName)
            //    .ThenBy(a => a.LastName)
            //    .AsQueryable();

            var collectionBeforePaging = context.Authors
                .ApplySort(pagination
                .OrderBy, propertyMappingService
                .GetPropertyMapping<ReadAuthor, Author>());

            if (!string.IsNullOrEmpty(pagination.Genre))
            {
                var genreForWhereClause = pagination.Genre.Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.Genre.ToLowerInvariant() == genreForWhereClause);
            }

            if (!string.IsNullOrEmpty(pagination.SearchQuery))
            {
                var searchQueryForWhereClause = pagination.SearchQuery.Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.Genre.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.FirstName.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.LastName.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }

            return PagedList<Author>.Create(
                collectionBeforePaging, pagination.PageNumber, pagination.PageSize);
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return context.Authors
                .Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .OrderBy(a => a.LastName)
                .ToList();
        }

        public void UpdateAuthor(Author author) { }

        public Book GetBookForAuthor(Guid authorId, Guid bookId)
        {
            return context.Books.Where(
                b => b.AuthorId == authorId && b.Id == bookId)
                .FirstOrDefault();
        }

        public IEnumerable<Book> GetBooksForAuthor(Guid authorId)
        {
            return context.Books.Where(
                b => b.AuthorId == authorId).OrderBy(b => b.Title)
                .ToList();
        }

        public void UpdateBookForAuthor(Book book) { }

        public bool Save()
        {
            return (context.SaveChanges() >= 0);
        }
    }
}
