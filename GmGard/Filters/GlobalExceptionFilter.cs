using GmGard.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly IHostingEnvironment _env;

        public GlobalExceptionFilter(ILoggerFactory logger, IHostingEnvironment env, IModelMetadataProvider modelMetadataProvider)
        {
            _logger = logger.CreateLogger("Global Exception Filter");
            _modelMetadataProvider = modelMetadataProvider;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new EventId(1), context.Exception, context.Exception.Message);
            if (!_env.IsProduction())
            {
                return;
            }
            var result = new ViewResult { ViewName = "Error", StatusCode = 500 };
            result.ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState) {
                Model = new ErrorContextModel {
                    Exception = context.Exception,
                    ControllerActionPath = $"{context.RouteData.Values["Controller"]}/{context.RouteData.Values["Action"]}"
                }
            };
            context.ExceptionHandled = true; // mark exception as handled
            context.Result = result;
        }
    }
}
