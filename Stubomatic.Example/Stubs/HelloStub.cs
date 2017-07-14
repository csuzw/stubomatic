using Stubomatic.Example.Dtos;

namespace Stubomatic.Example.Stubs
{
    public class HelloStub
    {
        public HelloWorldResult HelloWorld(string name)
        {
            return new HelloWorldResult("Stub");
        }

        public string LoudHelloWorld(string name)
        {
            return "HELLO STUB!";
        }
    }
}
