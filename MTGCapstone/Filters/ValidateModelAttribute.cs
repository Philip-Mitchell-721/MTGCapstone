using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace MTGCapstone.API.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
        //    if (actionContext.ModelState.IsValid == false)
        //    {
        //        actionContext.Response = actionContext.Request.CreateErrorResponse(
        //            HttpStatusCode.BadRequest, actionContext.ModelState);
        //    }
        }
    }
}
