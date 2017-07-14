using System;

namespace Stubomatic
{
    public interface IControllerTypeResolver
    {
        Type GetStubType(Type type);
    }
}
