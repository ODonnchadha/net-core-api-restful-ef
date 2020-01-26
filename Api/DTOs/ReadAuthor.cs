using System;

namespace Api.DTOs
{
    public class ReadAuthor
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Genre { get; set; }
    }
}
