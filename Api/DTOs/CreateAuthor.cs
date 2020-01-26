using System;
using System.Collections.Generic;

namespace Api.DTOs
{
    public class CreateAuthor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string Genre { get; set; }
        public ICollection<CreateBook> Books { get; set; } = new List<CreateBook>();
    }
}
