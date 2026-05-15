using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        return Ok(new object[] {
            new { Key = "low_inventory", Label = "Inventario bajo", Category = "Inventory", Value = true },
            new { Key = "daily_summary", Label = "Resumen de ventas", Category = "Sales", Value = false }
        });
    }

    [HttpPut("settings/{key}")]
    public IActionResult UpdateSetting(string key, [FromBody] UpdateSettingRequest req)
    {
        return Ok();
    }

    public class UpdateSettingRequest
    {
        public bool Value { get; set; }
    }
}
