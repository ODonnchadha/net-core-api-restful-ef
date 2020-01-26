namespace Api.Helpers
{
    public class AuthorsResourceParameters
    {
        private const int MAX_PAGE_SIZE = 20;
        private int pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get { return pageSize; } set { pageSize = (value > MAX_PAGE_SIZE) ? MAX_PAGE_SIZE : value; } }
        public string Genre { get; set; }
        public string SearchQuery { get; set; }
        public string OrderBy { get; set; } = "Name";
        public string Fields { get; set; }
    }
}
