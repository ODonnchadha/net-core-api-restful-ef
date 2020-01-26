using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;

namespace Api.Helpers
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestHeaderMatchesMediaType : Attribute, IActionConstraint
    {
        private readonly string requestHeaderToMatch;
        private readonly string[] mediaTypes;

        public RequestHeaderMatchesMediaType(string requestHeaderToMatch, string[] mediaTypes)
        {
            this.requestHeaderToMatch = requestHeaderToMatch;
            this.mediaTypes = mediaTypes;
        }

        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;

            if (!requestHeaders.ContainsKey(requestHeaderToMatch))
            {
                return false;
            }

            foreach(var mediaType in mediaTypes)
            {
                var mediaTypeMatches = string.Equals(
                    requestHeaders[requestHeaderToMatch].ToString(), mediaType, StringComparison.OrdinalIgnoreCase);

                if (mediaTypeMatches)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
