using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace Stubomatic
{
    /// <summary>
    /// Ensures dynamic stub assembly is generated and linked
    /// </summary>
    internal class StubomaticAssembliesResolver : IAssembliesResolver
    {
        private readonly IAssembliesResolver _resolver;
        private readonly Lazy<Assembly> _stubAssembly;

        public StubomaticAssembliesResolver(IAssembliesResolver resolver, StubomaticOptions options)
        {
            _resolver = resolver;
            _stubAssembly = new Lazy<Assembly>(() => _resolver.GetAssemblies().SelectMany(a => a.GetTypes().Where(i => typeof(ApiController).IsAssignableFrom(i))).CreateStubAssembly(options));
        }

        public ICollection<Assembly> GetAssemblies()
        {
            var assemblies = new List<Assembly>(_resolver.GetAssemblies());
            if (!assemblies.Contains(_stubAssembly.Value)) assemblies.Add(_stubAssembly.Value);
            return assemblies;
        }
    }
}
