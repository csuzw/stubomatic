namespace Stubomatic.Example.Dtos
{
    public class HelloWorldResult
    {
        public string Message { get; set; }

        public HelloWorldResult()
        {
        }

        public HelloWorldResult(string name)
        {
            Message = "Hello " + name + "!";
        }
    }
}
