namespace Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tester;

    [Route("api/v1")]
    [ApiController]
    public class TesterController : Controller
    {
        private readonly TesterService _testService;

        public TesterController(TesterService testService)
        {
            _testService = testService;
        }
        /// <summary>
        /// Example https://github.com/Fareman/TestTAP
        /// </summary>
        /// <param name="gitZipUrl">Ссылка на репозиторий с тестовым заданием.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task TestApi(string gitZipUrl)
        {
            if (gitZipUrl.Length == 0)
            {
                Console.WriteLine(BadRequest(gitZipUrl));
            }
            await _testService.TestAsync(gitZipUrl);
        }
    }
}
