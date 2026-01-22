using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SocialPostApi.Models;

namespace SocialPostApi.Filters
{
    public class ApiResponseFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                context.Result = new JsonResult(new ApiResponse
                {
                    Success = false,
                    Message = "Exception thrown",
                    Data = null,
                    Error = new ApiError
                    {
                        StatusCode = 500,
                        ErrorCode = "SERVER_EXCEPTION",
                        ErrorMessage = context.Exception.Message,
                        Details = context.Exception.StackTrace
                    }
                });

                context.ExceptionHandled = true;
                return;
            }

            if (context.Result is ObjectResult obj)
            {
                context.Result = new JsonResult(new ApiResponse
                {
                    Success = (obj.StatusCode is >= 200 and < 300),
                    Message = "OK",
                    Data = obj.Value,
                    Error = null
                });
            }
        }
    }
}
