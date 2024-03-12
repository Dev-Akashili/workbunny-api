using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkBunny.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> TestControllerAuth() 
        => await Task.FromResult(Ok("Auth Works Fine!"));
}