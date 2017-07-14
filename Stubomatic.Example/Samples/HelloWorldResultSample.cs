using Stubomatic.Example.Dtos;

namespace Stubomatic.Example.Samples
{
    public class HelloWorldResultSample : ISample<HelloWorldResult>
    {
        public HelloWorldResult Sample
        {
            get
            {
                return new HelloWorldResult("Sample");
            }
        }
    }
}
