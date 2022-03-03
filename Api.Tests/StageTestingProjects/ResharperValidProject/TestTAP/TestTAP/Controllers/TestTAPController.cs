namespace TestTAP.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контроллер.
/// </summary>
[ApiController]
[Route("[controller]")]
public class TestTAPController : ControllerBase
{
    /// <summary>
    /// Получение сообщения.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public string Get()
    {
        return "Nice start!";
    }
}