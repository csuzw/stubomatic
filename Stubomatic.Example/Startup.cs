using System.Web.Http;
using Owin;

namespace Stubomatic.Example
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();
            configuration.UseStubomatic(new StubomaticOptions
            {
                ResolveFrom = ResolveFrom.ResponseType,
                ResponseTypeResolver = new SampleResponseTypeResolver(),
                MissingStubHandling = MissingStubHandling.Message
            });
            configuration.MapHttpAttributeRoutes();
            configuration.EnsureInitialized();

            app.UseWebApi(configuration);
        }
    }
}
