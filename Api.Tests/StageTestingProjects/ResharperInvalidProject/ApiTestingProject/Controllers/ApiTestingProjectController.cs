using Microsoft.AspNetCore.Mvc;

namespace ApiTestingProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiTestingProjectController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Nice start!";
        }
    }
}
