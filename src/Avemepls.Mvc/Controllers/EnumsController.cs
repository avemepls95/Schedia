using Avemepls.Infrastructure.Enums;
using Avemepls.Infrastructure.Enums.Models;
using Avemepls.Mvc.Errors;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avemepls.Mvc.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName = "common")]
[Route("api/[controller]")]
public class EnumsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EnumModel[]))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestErrorModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public OkObjectResult GetEnums([FromServices] EnumsService enumsService)
    {
        return Ok(enumsService.GetAllEnums());
    }
}