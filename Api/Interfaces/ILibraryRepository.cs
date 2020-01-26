using Api.Helpers;
using Api.Models;
using System;
using System.Collections.Generic;

namespace Api.Interfaces
{
    public interface ILibraryRepository
    {
        void AddAuthor(Author author);
        void AddBookForAuthor(Guid authorId, Book book);
        bool AuthorExists(Guid authorId);
        void DeleteAuthor(Author author);
        void DeleteBook(Book book);
        bool Save();
        void UpdateAuthor(Author author);
        void UpdateBookForAuthor(Book book);
        Author GetAuthor(Guid authorId);
        Book GetBookForAuthor(Guid authorId, Guid bookId);
        PagedList<Author> GetAuthors(AuthorsResourceParameters pagination);
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);
        IEnumerable<Book> GetBooksForAuthor(Guid authorId);
    }
}
