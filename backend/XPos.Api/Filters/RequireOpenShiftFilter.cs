using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XPos.Domain.Interfaces;

namespace XPos.Api.Filters;

public class RequireOpenShiftFilter : IAsyncActionFilter
{
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly ICurrentUserService _currentUserService;

    public RequireOpenShiftFilter(ICashShiftRepository cashShiftRepository, ICurrentUserService currentUserService)
    {
        _cashShiftRepository = cashShiftRepository;
        _currentUserService = currentUserService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = _currentUserService.UserId;
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;

        if (userId <= 0 || !activeWarehouseId.HasValue)
        {
            context.Result = new ObjectResult(new { message = "Debes abrir un turno de caja antes de registrar ventas." })
            {
                StatusCode = 403
            };
            return;
        }

        var activeShift = await _cashShiftRepository.GetActiveShiftAsync(userId, activeWarehouseId.Value);
        if (activeShift == null)
        {
            context.Result = new ObjectResult(new { message = "Debes abrir un turno de caja antes de registrar ventas." })
            {
                StatusCode = 403
            };
            return;
        }

        // Guardar el id del turno activo para que el controlador lo pueda asignar a la venta
        context.HttpContext.Items["ActiveCashShiftId"] = activeShift.Id;

        await next();
    }
}
