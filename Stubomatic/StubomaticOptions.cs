namespace Stubomatic
{
    public class StubomaticOptions
    {
        public string RoutePrefix { get; set; }
        public ResolveFrom ResolveFrom { get; set; }
        public IControllerTypeResolver ControllerTypeResolver { get; set; }
        public IResponseTypeResolver ResponseTypeResolver { get; set; }
        public MissingStubHandling MissingStubHandling { get; set; }
        public string MissingStubHandlingMessage { get; set; }

        public StubomaticOptions()
        {
            RoutePrefix = "stub/";
            ResolveFrom = ResolveFrom.ControllerType;
            ControllerTypeResolver = new DefaultControllerTypeResolver();
            ResponseTypeResolver = null;
            MissingStubHandling = MissingStubHandling.NotFound;
            MissingStubHandlingMessage = "Stub not implemented for this endpoint.";
        }
    }
}
