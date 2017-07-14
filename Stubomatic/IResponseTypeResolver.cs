using System;
using System.Reflection.Emit;

namespace Stubomatic
{
    public interface IResponseTypeResolver
    {
        Action<ILGenerator> GetILGenerator(Type responseType);
    }
}
