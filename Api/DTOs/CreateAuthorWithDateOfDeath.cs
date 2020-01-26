using System;
using System.Collections.Generic;

namespace Api.DTOs
{
    public class CreateAuthorWithDateOfDeath
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public DateTimeOffset? DateOfDeath { get; set; }
        public string Genre { get; set; }
    }
}
