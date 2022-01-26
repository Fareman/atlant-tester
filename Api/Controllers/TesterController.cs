namespace Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tester;

    [Route("api/v1")]
    [ApiController]
    public class TesterController : Controller
    {
        private readonly ITesterService _testService;

        public TesterController(ITesterService testService)
        {
            _testService = testService;
        }

        [HttpPost]
        public async Task TestApi(string gitZipUri)
        {
            if (gitZipUri.Length == 0)
            {
                Console.WriteLine(BadRequest(gitZipUri));
            }
            await _testService.TestAsync(gitZipUri);
        }
    }
}
