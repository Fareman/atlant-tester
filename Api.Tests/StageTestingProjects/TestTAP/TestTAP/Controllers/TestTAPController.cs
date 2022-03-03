using Microsoft.AspNetCore.Mvc;

namespace TestTAP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestTAPController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Nice start!";
        }
    }
}
