using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace Stubomatic
{
    public static class HttpConfigurationExtensions
    {
        public static void UseStubomatic(this HttpConfiguration configuration, StubomaticOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (options.ResolveFrom == ResolveFrom.ControllerType && options.ControllerTypeResolver == null) throw new ArgumentException("options.ControllerTypeResolver cannot be null");
            if (options.ResolveFrom == ResolveFrom.ResponseType && options.ResponseTypeResolver == null) throw new ArgumentException("options.ResponseTypeResolver cannot be null");

            configuration.Services.Replace(typeof(IAssembliesResolver), new StubomaticAssembliesResolver(configuration.Services.GetAssembliesResolver(), options));
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), new StubomaticHttpControllerTypeResolver(configuration.Services.GetHttpControllerTypeResolver()));
        }
    }
}
