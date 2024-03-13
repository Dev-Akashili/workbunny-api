using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkBunny.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TestController : ControllerBase
{
    [Authorize(Policy = "RequireAdmin")]
    [HttpGet("admin")]
    public async Task<ActionResult<string>> TestAdmin() 
        => await Task.FromResult(Ok("Admin Works Fine!"));
    
    [Authorize]
    [HttpGet("basic")]
    public async Task<ActionResult<string>> TestBasic() 
        => await Task.FromResult(Ok("Basic Works Fine!"));
}
