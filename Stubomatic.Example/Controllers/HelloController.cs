using System.Web.Http;
using System.Web.Http.Description;
using Stubomatic.Example.Dtos;

namespace Stubomatic.Example.Controllers
{
    public class HelloController : ApiController
    {
        [HttpGet]
        [Route("hello/{name}")]
        [ResponseType(typeof(HelloWorldResult))]
        public IHttpActionResult HelloWorld(string name)
        {
            var safeName = !string.IsNullOrWhiteSpace(name) ? name : "World";

            var result = new HelloWorldResult(safeName);

            return Ok(result);
        }

        [HttpGet]
        [Route("hello/{name}/loud")]
        [ResponseType(typeof(LoudHelloWorldResult))]
        public IHttpActionResult LoudHelloWorld(string name)
        {
            var safeName = !string.IsNullOrWhiteSpace(name) ? name.ToUpper() : "WORLD";

            var result = new LoudHelloWorldResult(safeName);

            return Ok(result);
        }

        [HttpGet]
        [Route("hello/{name}/quiet")]
        [ResponseType(typeof(QuietHelloWorldResult))]
        public IHttpActionResult QuietHelloWorld(string name)
        {
            var safeName = !string.IsNullOrWhiteSpace(name) ? name.ToLower() : "world";

            var result = new QuietHelloWorldResult(safeName);

            return Ok(result);
        }
    }
}
