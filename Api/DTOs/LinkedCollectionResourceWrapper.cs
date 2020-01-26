using System.Collections.Generic;

namespace Api.DTOs
{
    public class LinkedCollectionResourceWrapper<T> : LinkedResourceBase where T : LinkedResourceBase
    {
        public IEnumerable<T> Value { get; set; }

        public LinkedCollectionResourceWrapper(IEnumerable<T> value)
        {
            this.Value = value;
        }
    }
}
