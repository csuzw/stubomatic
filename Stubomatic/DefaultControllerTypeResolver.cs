using System;

namespace Stubomatic
{
    public class DefaultControllerTypeResolver : IControllerTypeResolver
    {
        public Type GetStubType(Type controllerType)
        {
            var stubName = controllerType.FullName.Replace("Controller", "Stub");
            return controllerType.Assembly.GetType(stubName);
        }
    }
}
