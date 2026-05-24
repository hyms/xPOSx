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
    private readonly IClientRepository _clientRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly SiatSoapService _siatSoapService;

    public SaleService(
        IUnitOfWork uow, 
        ISaleRepository saleRepository,
        IVoucherRepository voucherRepository,
        IInventoryRepository inventoryRepository,
        IPaymentRepository paymentRepository,
        IUnitRepository unitRepository,
        UnitConversionService unitConversionService,
        ICashShiftRepository cashShiftRepository,
        IClientRepository clientRepository,
        IWarehouseRepository warehouseRepository,
        SiatSoapService siatSoapService)
    {
        _uow = uow;
        _saleRepository = saleRepository;
        _voucherRepository = voucherRepository;
        _inventoryRepository = inventoryRepository;
        _paymentRepository = paymentRepository;
        _unitRepository = unitRepository;
        _unitConversionService = unitConversionService;
        _cashShiftRepository = cashShiftRepository;
        _clientRepository = clientRepository;
        _warehouseRepository = warehouseRepository;
        _siatSoapService = siatSoapService;
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

    public async Task<long> CreateOnlineSaleAsync(Sale sale)
    {
        if (sale == null)
        {
            throw new ArgumentNullException(nameof(sale));
        }

        _uow.BeginTransaction();
        try
        {
            if (sale.ClientId == 0)
            {
                var clients = await _clientRepository.GetAllAsync();
                var firstClient = clients.FirstOrDefault();
                if (firstClient == null)
                {
                    var newClient = new Client
                    {
                        Name = "Cliente General Web",
                        NitCi = "0",
                        Phone = "000000",
                        Email = "web@client.com"
                    };
                    var newClientId = await _clientRepository.CreateAsync(newClient);
                    sale.ClientId = newClientId;
                }
                else
                {
                    sale.ClientId = firstClient.Id;
                }
            }

            if (sale.WarehouseId == 0)
            {
                var warehouses = await _warehouseRepository.GetAllAsync();
                var firstWarehouse = warehouses.FirstOrDefault();
                if (firstWarehouse != null)
                {
                    sale.WarehouseId = firstWarehouse.Id;
                }
            }

            sale.IsPos = false;
            sale.Status = "PENDING_VERIFICATION";
            sale.PaymentStatus = "unpaid";
            sale.Date = DateTime.UtcNow;
            sale.CreatedBy = null; // Public sale, no authenticated user

            if (string.IsNullOrEmpty(sale.Ref))
            {
                sale.Ref = $"WEB-{DateTime.Now:yyyyMMddHHmmss}";
            }

            // Use the repository to create the sale and detail rows
            var saleId = await _saleRepository.CreateAsync(sale);

            _uow.Commit();
            return saleId;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> ApproveOnlineSaleAsync(long id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        if (sale == null)
        {
            return false;
        }

        if (sale.Status != "PENDING_VERIFICATION")
        {
            throw new InvalidOperationException("El pedido no se encuentra en estado pendiente de verificación");
        }

        _uow.BeginTransaction();
        try
        {
            // 1. Update status
            await _saleRepository.UpdateStatusAsync(id, "PROCESSING", "paid");

            // 2. Discount stock
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
                    
                    // Subtract from inventory
                    await _inventoryRepository.UpdateStockAsync(detail.ProductId, sale.WarehouseId, -baseQuantity);
                }
            }

            _uow.Commit();
            return true;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> VerifyOnlineSaleAsync(long id, long userId, long cashShiftId)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        if (sale == null)
        {
            return false;
        }

        if (sale.Status != "PENDING_VERIFICATION")
        {
            throw new InvalidOperationException("La venta no se encuentra en estado pendiente de verificación.");
        }

        _uow.BeginTransaction();
        try
        {
            // 1. Update status to 'PAID', and assign the cashier's userId & open cashShiftId
            await _saleRepository.UpdateVerifyStatusAsync(id, "PAID", "paid", userId, cashShiftId);

            // 2. Discount stock
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
                    
                    // Subtract from inventory
                    await _inventoryRepository.UpdateStockAsync(detail.ProductId, sale.WarehouseId, -baseQuantity);
                }
            }

            // 3. Register the payment in the system (payment_sales)
            var paymentDto = new PaymentSaleDto
            {
                UserId = userId,
                Date = DateTime.UtcNow,
                Ref = "PAY-VERIFY-" + sale.Ref,
                SaleId = id,
                Amount = sale.GrandTotal,
                Reglement = "Manual Bank Receipt",
                CreatedBy = userId
            };
            await _paymentRepository.CreateSalePaymentAsync(paymentDto);

            _uow.Commit();

            // 4. Trigger the SIAT billing process (outside transaction for resilience)
            try
            {
                var soapEnvelope = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:siat=""http://siat.impuestos.gob.bo/"">
                    <soapenv:Header/>
                    <soapenv:Body>
                        <siat:recepcionFactura>
                            <nit>{sale.Nit ?? "0"}</nit>
                            <razonSocial>{sale.RazonSocial ?? "Sin Nombre"}</razonSocial>
                            <montoTotal>{sale.GrandTotal}</montoTotal>
                        </siat:recepcionFactura>
                    </soapenv:Body>
                </soapenv:Envelope>";
                
                // Trigger SIAT billing as pilot service
                await _siatSoapService.SendSiatSoapRequestAsync(
                    "https://pilotosiat.impuestos.gob.bo/v2/ServicioFacturacionComputarizada",
                    "recepcionFactura",
                    soapEnvelope,
                    timeoutSeconds: 3
                );
            }
            catch (Exception)
            {
                // Externals failure (like SIAT pilot service) must not compromise database transaction success.
                // We proceed since the database state was successfully committed.
            }

            return true;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> RejectOnlineSaleAsync(long id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        if (sale == null)
        {
            return false;
        }

        _uow.BeginTransaction();
        try
        {
            // If it was already approved (PROCESSING) and we are rejecting/cancelling, we must restore stock
            if (sale.Status == "PROCESSING" && sale.Details != null)
            {
                foreach (var detail in sale.Details)
                {
                    Unit? unit = null;
                    if (detail.SaleUnitId.HasValue)
                    {
                        unit = await _unitRepository.GetByIdAsync(detail.SaleUnitId.Value);
                    }

                    var baseQuantity = _unitConversionService.CalculateBaseQuantity(detail.Quantity, unit);
                    
                    // Add back to inventory
                    await _inventoryRepository.UpdateStockAsync(detail.ProductId, sale.WarehouseId, baseQuantity);
                }
            }

            // Update status to 'REJECTED' or 'cancelled'
            await _saleRepository.UpdateStatusAsync(id, "REJECTED", "unpaid");

            _uow.Commit();
            return true;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
