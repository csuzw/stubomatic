using System;
using System.Linq;

namespace Stubomatic
{
    public class DefaultResponseTypeResolver: IControllerTypeResolver
    {
        private readonly Type _openGenericType;

        public DefaultResponseTypeResolver(Type openGenericType)
        {
            if (!openGenericType.IsGenericTypeDefinition) throw new Exception("T must be an open generic type in GenericStubResponseTypeResolver<T>");

            _openGenericType = openGenericType;
        }

        public Type GetStubType(Type type)
        {
            var genericType = _openGenericType.MakeGenericType(type);
            return type.Assembly.GetTypes().FirstOrDefault(t => genericType.IsAssignableFrom(t));
        }
    }
}
