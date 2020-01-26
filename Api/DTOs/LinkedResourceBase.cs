using System.Collections.Generic;

namespace Api.DTOs
{
    public abstract class LinkedResourceBase
    {
        public List<Link> Links { get; set; } = new List<Link>();
    }
}
