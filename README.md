# Stubomatic

Library for automatically generating stubbed WebApi endpoints.  See `Stubomatic.Example.Startup.cs` for example usage.  Note that this currently only works for attribute routing.  By default if the real endpoint is http://localhost/hello/{name} then the stubbed endpoint will be http://localhost/stub/hello/{name}.


