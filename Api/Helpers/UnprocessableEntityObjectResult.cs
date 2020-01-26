using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Api.Helpers
{
    public class UnprocessableEntityObjectResult : ObjectResult
    {
        public UnprocessableEntityObjectResult(ModelStateDictionary modelState) 
            : base(new SerializableError(modelState))
        {
            if (null == modelState)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            StatusCode = 422;
        }
    }
}
