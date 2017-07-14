using System;

namespace Stubomatic
{
    public class StubNotFoundException : Exception
    {
        public StubNotFoundException() : base()
        {         
        }

        public StubNotFoundException(string message) : base(message)
        {
        }

        public StubNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
