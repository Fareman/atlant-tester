namespace Api.Controllers;

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
    ///     Example https://github.com/Fareman/ApiTestingProject
    /// </summary>
    /// <param name="gitUrl">Ссылка на репозиторий с тестовым заданием.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> TestApi(string gitUrl)
    {
        if (!gitUrl.Contains("https://github.com/"))
            return BadRequest("Invalid repository link");
        var report = await _testService.TestAsync(gitUrl);
        return Ok(report);
    }
}