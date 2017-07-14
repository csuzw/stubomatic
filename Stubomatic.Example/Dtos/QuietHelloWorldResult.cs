namespace Stubomatic.Example.Dtos
{
    public class QuietHelloWorldResult
    {
        public string Message { get; set; }

        public QuietHelloWorldResult()
        {            
        }

        public QuietHelloWorldResult(string name)
        {
            Message = "hello " + name.ToLower() + "!";
        }
    }
}
