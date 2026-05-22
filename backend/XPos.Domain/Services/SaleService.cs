using System;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class SaleService : ISaleService
{
    private readonly IUnitOfWork _uow;
    private readonly ISaleRepository _saleRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly UnitConversionService _unitConversionService;
    private readonly ICashShiftRepository _cashShiftRepository;

    public SaleService(
        IUnitOfWork uow, 
        ISaleRepository saleRepository,
        IVoucherRepository voucherRepository,
        IInventoryRepository inventoryRepository,
        IPaymentRepository paymentRepository,
        IUnitRepository unitRepository,
        UnitConversionService unitConversionService,
        ICashShiftRepository cashShiftRepository)
    {
        _uow = uow;
        _saleRepository = saleRepository;
        _voucherRepository = voucherRepository;
        _inventoryRepository = inventoryRepository;
        _paymentRepository = paymentRepository;
        _unitRepository = unitRepository;
        _unitConversionService = unitConversionService;
        _cashShiftRepository = cashShiftRepository;
    }

    public async Task<PagedResult<SaleReadDto>> GetAllAsync(PagingParams pagingParams)
    {
        return await _saleRepository.GetAllAsync(pagingParams);
    }

    public async Task<Sale?> GetByIdAsync(long id)
    {
        return await _saleRepository.GetByIdAsync(id);
    }

    public async Task<long> CreateSaleAsync(Sale sale, long userId)
    {
        // Business logic previously in Controller
        sale.UserId = userId;
        sale.CreatedBy = userId;

        if (string.IsNullOrEmpty(sale.Ref))
        {
            sale.Ref = $"SL-{DateTime.Now:yyyyMMddHHmmss}";
        }

        // 1. Iniciar la transacción
        _uow.BeginTransaction();
        try
        {
            // 2. Crear la Venta
            var saleId = await _saleRepository.CreateAsync(sale);
            sale.Id = saleId;

            // 3. Crear el Comprobante (Voucher) si aplica
            if (sale.Voucher != null)
            {
                sale.Voucher.SaleId = saleId;
                var voucherId = await _voucherRepository.CreateAsync(sale.Voucher);
                await _saleRepository.UpdateVoucherIdAsync(saleId, voucherId);
            }

            // 4. Actualizar Inventario y realizar conversiones
            foreach (var detail in sale.Details)
            {
                Unit? unit = null;
                if (detail.SaleUnitId.HasValue)
                {
                    unit = await _unitRepository.GetByIdAsync(detail.SaleUnitId.Value);
                }

                var baseQuantity = _unitConversionService.CalculateBaseQuantity(detail.Quantity, unit);
                
                // Restar del inventario
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, sale.WarehouseId, -baseQuantity);
            }

            // 5. Registrar el Pago si es POS y tiene monto pagado
            if (sale.PaidAmount > 0)
            {
                var paymentDto = new PaymentSaleDto
                {
                    UserId = sale.UserId,
                    Date = sale.Date,
                    Ref = "PAY-" + sale.Ref,
                    SaleId = saleId,
                    Amount = sale.PaidAmount,
                    Reglement = "Cash",
                    CreatedBy = sale.CreatedBy
                };
                await _paymentRepository.CreateSalePaymentAsync(paymentDto);
            }

            // 6. Confirmar la transacción
            _uow.Commit();
            return saleId;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteSaleAsync(long id, long userId)
    {
        _uow.BeginTransaction();
        try
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null)
            {
                _uow.Rollback();
                return false;
            }

            if (sale.CashShiftId.HasValue)
            {
                var shift = await _cashShiftRepository.GetByIdAsync(sale.CashShiftId.Value);
                if (shift != null && shift.Status == "CLOSED")
                {
                    throw new InvalidOperationException("No se puede eliminar la venta porque pertenece a un turno de caja cerrado.");
                }
            }

            // 1. Devolver los productos al inventario (restaurar stock)
            if (sale.Details != null)
            {
                foreach (var detail in sale.Details)
                {
                    Unit? unit = null;
                    if (detail.SaleUnitId.HasValue)
                    {
                        unit = await _unitRepository.GetByIdAsync(detail.SaleUnitId.Value);
                    }

                    var baseQuantity = _unitConversionService.CalculateBaseQuantity(detail.Quantity, unit);
                    
                    // Sumar de nuevo al inventario
                    await _inventoryRepository.UpdateStockAsync(detail.ProductId, sale.WarehouseId, baseQuantity);
                }
            }

            // 2. Eliminar (soft-delete) la venta
            var deleted = await _saleRepository.DeleteAsync(id, userId);
            
            _uow.Commit();
            return deleted;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
