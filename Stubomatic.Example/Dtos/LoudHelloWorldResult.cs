namespace Stubomatic.Example.Dtos
{
    public class LoudHelloWorldResult
    {
        public string Message { get; set; }

        public LoudHelloWorldResult()
        {
        }

        public LoudHelloWorldResult(string name)
        {
            Message = "HELLO " + name.ToUpper() + "!";
        }
    }
}
