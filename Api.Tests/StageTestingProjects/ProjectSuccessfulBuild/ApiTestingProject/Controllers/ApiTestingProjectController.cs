namespace ApiTestingProject.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Контроллер приложения.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ApiTestingProjectController : ControllerBase
{
    /// <summary>
    /// Метод get.
    /// </summary>
    /// <returns> Возвращает строку. </returns>
    [HttpGet]
    public string Get()
    {
        return "Nice start!";
    }
}