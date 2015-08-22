using Microsoft.Owin;
using NLog;
using Owin;
using WebRole1;

[assembly: OwinStartup(typeof(Startup))]

namespace WebRole1
{
    public class Startup
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void Configuration(IAppBuilder app)
        {
            // New code:
            app.Run(context =>
            {
                _logger.Info("{0}:{1}", context.Request.Method, context.Request.Path);
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Hello, world.");
            });
        }
    }
}
