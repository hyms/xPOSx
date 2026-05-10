using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class ReturnService : IReturnService
{
    private readonly IUnitOfWork _uow;
    private readonly ISaleReturnRepository _saleReturnRepository;
    private readonly IPurchaseReturnRepository _purchaseReturnRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly UnitConversionService _unitConversionService;
    private readonly IVoucherRepository _voucherRepository;

    public ReturnService(
        IUnitOfWork uow,
        ISaleReturnRepository saleReturnRepository,
        IPurchaseReturnRepository purchaseReturnRepository,
        IInventoryRepository inventoryRepository,
        IUnitRepository unitRepository,
        UnitConversionService unitConversionService,
        IVoucherRepository voucherRepository)
    {
        _uow = uow;
        _saleReturnRepository = saleReturnRepository;
        _purchaseReturnRepository = purchaseReturnRepository;
        _inventoryRepository = inventoryRepository;
        _unitRepository = unitRepository;
        _unitConversionService = unitConversionService;
        _voucherRepository = voucherRepository;
    }

    public async Task<long> CreateSaleReturnAsync(SaleReturn saleReturn)
    {
        _uow.BeginTransaction();
        try
        {
            var returnId = await _saleReturnRepository.CreateAsync(saleReturn);
            saleReturn.Id = returnId;

            // Crear el Comprobante (Voucher) si aplica
            if (saleReturn.Voucher != null)
            {
                saleReturn.Voucher.SaleReturnId = returnId;
                var voucherId = await _voucherRepository.CreateAsync(saleReturn.Voucher);
                await _saleReturnRepository.UpdateVoucherIdAsync(returnId, voucherId);
            }


            foreach (var detail in saleReturn.Details)
            {
                Unit? unit = null;
                if (detail.SaleUnitId.HasValue)
                {
                    unit = await _unitRepository.GetByIdAsync(detail.SaleUnitId.Value);
                }

                var baseQuantity = _unitConversionService.CalculateBaseQuantity(detail.Quantity, unit);

                // Sale Returns add back to the warehouse
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, saleReturn.WarehouseId, baseQuantity);
            }

            _uow.Commit();
            return returnId;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<long> CreatePurchaseReturnAsync(PurchaseReturn purchaseReturn)
    {
        _uow.BeginTransaction();
        try
        {
            var returnId = await _purchaseReturnRepository.CreateAsync(purchaseReturn);
            purchaseReturn.Id = returnId;

            // Crear el Comprobante (Voucher) si aplica
            if (purchaseReturn.Voucher != null)
            {
                purchaseReturn.Voucher.PurchaseReturnId = returnId;
                var voucherId = await _voucherRepository.CreateAsync(purchaseReturn.Voucher);
                await _purchaseReturnRepository.UpdateVoucherIdAsync(returnId, voucherId);
            }


            foreach (var detail in purchaseReturn.Details)
            {
                Unit? unit = null;
                if (detail.PurchaseUnitId.HasValue)
                {
                    unit = await _unitRepository.GetByIdAsync(detail.PurchaseUnitId.Value);
                }

                var baseQuantity = _unitConversionService.CalculateBaseQuantity(detail.Quantity, unit);

                // Purchase Returns subtract from the warehouse
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, purchaseReturn.WarehouseId, -baseQuantity);
            }

            _uow.Commit();
            return returnId;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
