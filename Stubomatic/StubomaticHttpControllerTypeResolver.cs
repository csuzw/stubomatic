using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Stubomatic
{
    /// <summary>
    /// Ensures dynamic assemblies are considered when resolving controller types
    /// </summary>
    internal class StubomaticHttpControllerTypeResolver : IHttpControllerTypeResolver
    {
        private readonly IHttpControllerTypeResolver _resolver;

        public StubomaticHttpControllerTypeResolver(IHttpControllerTypeResolver resolver)
        {
            _resolver = resolver;
        }

        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            var baseTypes = _resolver.GetControllerTypes(assembliesResolver);
            var stubTypes = assembliesResolver.GetAssemblies().Where(a => a.IsDynamic).SelectMany(a => a.GetTypes().Where(IsControllerType));
            return baseTypes.Concat(stubTypes).Distinct().ToList();
        }

        private bool IsControllerType(Type t)
        {
            return t.IsPublic && t.IsVisible && !t.IsAbstract && typeof(IHttpController).IsAssignableFrom(t);
        }
    }
}
