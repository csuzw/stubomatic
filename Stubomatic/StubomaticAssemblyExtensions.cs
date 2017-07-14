using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Http;
using System.Web.Http.Description;

namespace Stubomatic
{
    public static class StubomaticAssemblyExtensions
    {
        internal static Assembly CreateStubAssembly(this IEnumerable<Type> controllerTypes, StubomaticOptions options)
        {
            var assemblyBuilder = AppDomain.CurrentDomain.DefineStubAssembly();
            var moduleBuilder = assemblyBuilder.DefineStubModule();

            foreach (var controllerType in controllerTypes)
            {
                moduleBuilder.GetStubProxyType(controllerType, options);
            }

            //assemblyBuilder.Save("stubs.assembly.dll");

            return assemblyBuilder;
        }

        //internal static Dictionary<Type, Type> GetStubTypes(this IEnumerable<Type> controllerTypes, StubomaticOptions options)
        //{
        //    var assemblyBuilder = AppDomain.CurrentDomain.DefineStubAssembly();
        //    var moduleBuilder = assemblyBuilder.DefineStubModule();

        //    var types = new Dictionary<Type, Type>();
        //    foreach (var controllerType in controllerTypes)
        //    {
        //        var stubType = moduleBuilder.GetStubProxyType(controllerType, options);
        //        if (stubType != null) types.Add(controllerType, stubType);
        //    }

        //    return types;
        //}

        private static AssemblyBuilder DefineStubAssembly(this AppDomain domain)
        {
            var assemblyName = new AssemblyName("stubAsm_" + Guid.NewGuid().ToString("N"));
            return domain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);//AndSave);
        }

        private static ModuleBuilder DefineStubModule(this AssemblyBuilder assemblyBuilder)
        {
            return assemblyBuilder.DefineDynamicModule("core");//, "stubs.module.dll", true);
        }

        private static Type GetStubProxyType(this ModuleBuilder moduleBuilder, Type controllerType, StubomaticOptions options)
        {
            Type stubType = null;
            ConstructorInfo stubConstructor = null;
            if (options.ResolveFrom == ResolveFrom.ControllerType)
            {
                stubType = options.ControllerTypeResolver.GetStubType(controllerType);
                stubConstructor = (stubType != null) ? stubType.GetConstructor(Type.EmptyTypes) : null;
                if (stubConstructor == null) // no stub class with default constructor
                {
                    if (options.MissingStubHandling == MissingStubHandling.NotFound) return null;
                    if (options.MissingStubHandling == MissingStubHandling.Exception) throw new StubNotFoundException(controllerType.FullName);
                }
            }

            var stubTypeName = (stubConstructor != null) ? stubType.Name : controllerType.Name;
            var typeName = string.Format("{0}{1:N}Controller", stubTypeName, Guid.NewGuid());
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class, typeof(ApiController));

            FieldBuilder stubFieldBuilder = (stubConstructor != null) ? typeBuilder.DefineStubField(stubType) : null; // generate stub field
            typeBuilder.DefineConstructor(stubFieldBuilder, stubConstructor);                           // generate default constructor to initialise stub field

            foreach (var controllerMethod in controllerType.GetMethodsToProxy())                        // generate proxy methods
            {
                if (options.ResolveFrom == ResolveFrom.ControllerType) typeBuilder.DefineControllerTypeProxyMethod(controllerMethod, stubFieldBuilder, options);
                if (options.ResolveFrom == ResolveFrom.ResponseType)   typeBuilder.DefineResponseTypeProxyMethod(controllerMethod, options);
            }

            return typeBuilder.CreateType();
        }

        private static IEnumerable<MethodInfo> GetMethodsToProxy(this Type controllerType)
        {
            return controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(i => i.GetCustomAttribute<RouteAttribute>() != null);
        }

        private static FieldBuilder DefineStubField(this TypeBuilder typeBuilder, Type stubType)
        {
            return typeBuilder.DefineField("_stub", stubType, FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        private static ConstructorBuilder DefineConstructor(this TypeBuilder typeBuilder, FieldBuilder stubFieldBuilder, ConstructorInfo stubConstructor)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

            var ilGenerator = constructorBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);                                                          // push this on to stack
            ilGenerator.Emit(OpCodes.Call, ApiControllerDefaultConstructor.Value);                      // call base constructor
            ilGenerator.Emit(OpCodes.Nop);                                                              // because this is what the compiler generates

            if (stubFieldBuilder != null && stubConstructor != null)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);                                                      // push this on to stack
                ilGenerator.Emit(OpCodes.Newobj, stubConstructor);                                      // construct new instance of stub type
                ilGenerator.Emit(OpCodes.Stfld, stubFieldBuilder);                                      // assign instance to field
            }

            ilGenerator.Emit(OpCodes.Ret);                                                              // finish

            return constructorBuilder;
        }

        private static readonly Lazy<MethodInfo> ApiControllerOkMethod = new Lazy<MethodInfo>(() => typeof(ApiController).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(i => i.Name == "Ok" && i.IsGenericMethod));
        private static readonly Lazy<ConstructorInfo> ApiControllerDefaultConstructor = new Lazy<ConstructorInfo>(() => typeof(ApiController).GetConstructor(BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[0], null));

        private static MethodBuilder DefineControllerTypeProxyMethod(this TypeBuilder typeBuilder, MethodInfo controllerMethod, FieldBuilder stubFieldBuilder, StubomaticOptions options)
        {
            var parameterTypes = controllerMethod.GetParameterTypes();
            var stubMethod = (stubFieldBuilder != null) ? stubFieldBuilder.FieldType.GetMethod(controllerMethod.Name, parameterTypes) : null;
            var stubExists = stubFieldBuilder != null && stubMethod != null;
            if (!stubExists)
            {
                if (options.MissingStubHandling == MissingStubHandling.NotFound) return null;
                if (options.MissingStubHandling == MissingStubHandling.Exception) throw new StubNotFoundException((controllerMethod.DeclaringType != null) ? controllerMethod.DeclaringType.FullName + "." + controllerMethod.Name : controllerMethod.Name); // no corresponding stub method
            }

            var returnType = (stubExists) ? stubMethod.ReturnType : typeof(string);
            var stubber = (stubExists) ?
                new Action<ILGenerator>((ilGenerator) =>
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);                                                      // push this on to stack
                    ilGenerator.Emit(OpCodes.Ldfld, stubFieldBuilder);                                      // push stub field on to stack
                    for (int i = 1; i <= parameterTypes.Length; i++)
                    {
                        ilGenerator.Emit(OpCodes.Ldarg, i);                                                 // push parameters onto stack
                    }
                    ilGenerator.Emit(OpCodes.Call, stubMethod);                                             // call stub method            
                }) : null;

            return typeBuilder.DefineProxyMethod(controllerMethod, options, returnType, stubber);
        }

        private static MethodBuilder DefineResponseTypeProxyMethod(this TypeBuilder typeBuilder, MethodInfo controllerMethod, StubomaticOptions options)
        {
            var responseTypeAttribute = controllerMethod.GetCustomAttribute<ResponseTypeAttribute>();
            var responseType = responseTypeAttribute != null ? responseTypeAttribute.ResponseType : null;
            var stubber = options.ResponseTypeResolver.GetILGenerator(responseType);
            var stubExists = stubber != null;
            if (!stubExists)
            {
                if (options.MissingStubHandling == MissingStubHandling.NotFound) return null;
                if (options.MissingStubHandling == MissingStubHandling.Exception) throw new StubNotFoundException((controllerMethod.DeclaringType != null) ? controllerMethod.DeclaringType.FullName + "." + controllerMethod.Name : controllerMethod.Name); // no corresponding response type stub
            }

            var returnType = (stubExists) ? responseType : typeof(string);

            return typeBuilder.DefineProxyMethod(controllerMethod, options, returnType, stubber);

        }

        private static MethodBuilder DefineProxyMethod(this TypeBuilder typeBuilder, MethodInfo controllerMethod, StubomaticOptions options, Type returnType, Action<ILGenerator> stubber)
        {
            var methodBuilder = typeBuilder.DefineMethod(controllerMethod.Name, controllerMethod.Attributes, controllerMethod.CallingConvention, typeof(IHttpActionResult), controllerMethod.GetParameterTypes());
            foreach (var parameter in controllerMethod.GetParameters())
            {
                methodBuilder.DefineParameter(parameter.Position + 1, parameter.Attributes, parameter.Name); // wtf is Position being inconsistently 0 or 1 based!
            }
            foreach (var attribute in controllerMethod.GetCustomAttributesData())
            {
                var attributeBuilder = attribute.ToAttributeBuilder(options);
                if (attributeBuilder == null) continue;
                methodBuilder.SetCustomAttribute(attributeBuilder);                                     // copy custom attribute
            }
            
            var okMethod = ApiControllerOkMethod.Value.MakeGenericMethod(returnType);

            var ilGenerator = methodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);                                                          // push this on to stack
            if (stubber != null)
            {
                stubber(ilGenerator);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldstr, options.MissingStubHandlingMessage);
            }
            ilGenerator.Emit(OpCodes.Call, okMethod);                                                   // call Ok method
            ilGenerator.Emit(OpCodes.Ret);                                                              // finish

            return methodBuilder;
        }

        private static CustomAttributeBuilder ToAttributeBuilder(this CustomAttributeData data, StubomaticOptions options)
        {
            if (data == null || data.NamedArguments == null) return null;

            var constructorArguments = new List<object>();

            var routeAttributeTemplateParameterIndex = -1;
            var routeAttributeType = typeof(RouteAttribute);
            if (data.AttributeType.IsSubclassOf(routeAttributeType) || data.AttributeType == routeAttributeType)
            {
                var templateParameter = data.Constructor.GetParameters().FirstOrDefault(p => p.Name == "template");
                if (templateParameter != null) routeAttributeTemplateParameterIndex = templateParameter.Position;
            }

            var index = 0;
            foreach (var ctorArg in data.ConstructorArguments)
            {
                var argValue = ctorArg.Value;  
                if (routeAttributeTemplateParameterIndex == index) argValue = options.RoutePrefix + (string)argValue;
                constructorArguments.Add(argValue);
                index += 1;
            }

            var propertyArguments = new List<PropertyInfo>();
            var propertyArgumentValues = new List<object>();
            var fieldArguments = new List<FieldInfo>();
            var fieldArgumentValues = new List<object>();
            foreach (var namedArg in data.NamedArguments)
            {
                var fi = namedArg.MemberInfo as FieldInfo;
                var pi = namedArg.MemberInfo as PropertyInfo;

                if (fi != null)
                {
                    fieldArguments.Add(fi);
                    fieldArgumentValues.Add(namedArg.TypedValue.Value);
                }
                else if (pi != null)
                {
                    propertyArguments.Add(pi);
                    propertyArgumentValues.Add(namedArg.TypedValue.Value);
                }
            }
            return new CustomAttributeBuilder(
              data.Constructor,
              constructorArguments.ToArray(),
              propertyArguments.ToArray(),
              propertyArgumentValues.ToArray(),
              fieldArguments.ToArray(),
              fieldArgumentValues.ToArray());
        }

        private static Type[] GetParameterTypes(this MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Select(i => i.ParameterType).ToArray();
        }
    }
}
