using Stubomatic.Example.Dtos;

namespace Stubomatic.Example.Samples
{
    public class LoudHelloWorldResultSample : ISample<LoudHelloWorldResult>
    {
        public LoudHelloWorldResult Sample
        {
            get
            {
                return new LoudHelloWorldResult("Sample");
            }
        }
    }
}
