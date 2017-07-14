using System;
using System.Linq;
using System.Reflection.Emit;
using Stubomatic.Example.Samples;

namespace Stubomatic.Example
{
    public class SampleResponseTypeResolver: IResponseTypeResolver
    {
        public Action<ILGenerator> GetILGenerator(Type responseType)
        {
            var genericType = typeof(ISample<>).MakeGenericType(responseType);
            var sampleType = responseType.Assembly.GetTypes().FirstOrDefault(t => genericType.IsAssignableFrom(t));
            if (sampleType == null) return null;

            var sampleConstructor = sampleType.GetConstructor(Type.EmptyTypes);
            if (sampleConstructor == null) return null;

            var sampleProperty = sampleType.GetProperty("Sample");
            if (sampleProperty == null) return null;

            var samplePropertyGetter = sampleProperty.GetGetMethod();
            if (samplePropertyGetter == null) return null;

            return (ilGenerator) =>
            {
                ilGenerator.Emit(OpCodes.Newobj, sampleConstructor);        // construct new instance of sample type
                ilGenerator.Emit(OpCodes.Call, samplePropertyGetter);       // call sample propery getter            
            };
        }
    }
}
