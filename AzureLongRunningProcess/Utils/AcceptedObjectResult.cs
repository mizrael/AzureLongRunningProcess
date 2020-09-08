using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AzureLongRunningProcess.Utils
{
    /// <summary>
    /// https://github.com/Azure/azure-functions-host/issues/4267
    /// </summary>
    public class AcceptedObjectResult : ObjectResult
    {
        private readonly string _location;

        public AcceptedObjectResult(string location, object value) : base(value)
        {
            _location = location;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = 202;

            var uri = EntityUriHelpers.BuildAbsolute(_location, context.HttpContext.Request);

            context.HttpContext.Response.Headers.Add(@"Location", uri.ToString());
            
            return base.ExecuteResultAsync(context);
        }
    }
}