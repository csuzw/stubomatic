# Route Mapping

My initial approach to this provided a hook into HttpConfiguration after `MapHttpAttributeRoutes` had been run.  
This iterated over routes, generated stubs in a similar way to the current approach but unfortunately the dynamic assembly was created after assembly/controller registration.
I've included some of the code I used for the original approach here just because I don't want to lose it.

```
public static void MapStubRoutes(this HttpConfiguration configuration)
{
    var previousInitializer = configuration.Initializer;
    configuration.Initializer = config =>
    {
        previousInitializer(config);

        var routes = config.Routes.Flatten().ToList();

        var controllerTypes = routes.Select(i => i.GetControllerMethodInfo().DeclaringType).Distinct();
        var stubTypes = controllerTypes.GetStubTypes();

        foreach (var route in routes)
        {
            var stubMethodInfo = stubTypes.GetMethodInfo(route.GetControllerMethodInfo());
            if (stubMethodInfo == null || stubMethodInfo.DeclaringType == null) continue;

            configuration.AddStubRoute(route, stubMethodInfo);

            Console.WriteLine(route.RouteTemplate);
        }
    };
}

private static IEnumerable<IHttpRoute> Flatten(this IEnumerable<IHttpRoute> routes)
{
    foreach (var route in routes)
    {
        var enumerable = route as IEnumerable<IHttpRoute>;
        if (enumerable == null)
        {
            yield return route;
        }
        else
        {
            foreach (var r in enumerable.Flatten())
            {
                yield return r;
            }
        }
    }
}

private static MethodInfo GetControllerMethodInfo(this IHttpRoute route)
{
    var actionDescriptors = route.DataTokens["actions"] as HttpActionDescriptor[];
    if (actionDescriptors == null || actionDescriptors.Length <= 0) return null;

    var reflectedActionDescriptor = actionDescriptors[0] as ReflectedHttpActionDescriptor;

    return reflectedActionDescriptor?.MethodInfo;
}

private static MethodInfo GetMethodInfo(this Dictionary<Type, Type> stubTypes, MethodInfo controllerMethodInfo)
{
    if (controllerMethodInfo.DeclaringType == null) return null;

    Type stubType;
    if (!stubTypes.TryGetValue(controllerMethodInfo.DeclaringType, out stubType)) return null;

    var stubMethod = stubType.GetMethod(controllerMethodInfo.Name, controllerMethodInfo.GetParameters().Select(i => i.ParameterType).ToArray());
    if (stubMethod == null) return null;

    return stubMethod;
}

private static void AddStubRoute(this HttpConfiguration configuration, IHttpRoute route, MethodInfo stubMethodInfo)
{
    if (stubMethodInfo.DeclaringType == null) return;

    var stubDataTokens = new Dictionary<string, object>(route.DataTokens.Where(i => i.Key != "actions").ToDictionary(i => i.Key, i => i.Value))
            {
                {
                    "actions",
                    new HttpActionDescriptor[]
                    {
                        new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(configuration, stubMethodInfo.DeclaringType.FullName, stubMethodInfo.DeclaringType), stubMethodInfo)
                    }
                }
            };

    var stubRoute = configuration.Routes.CreateRoute("stub/" + route.RouteTemplate, route.Defaults, route.Constraints, stubDataTokens, route.Handler);
    configuration.Routes.Insert(0, stubMethodInfo.Name, stubRoute);
}
```