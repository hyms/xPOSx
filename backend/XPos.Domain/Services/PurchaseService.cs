using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IUnitOfWork _uow;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly UnitConversionService _unitConversionService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IVoucherRepository _voucherRepository;

    public PurchaseService(
        IUnitOfWork uow,
        IPurchaseRepository purchaseRepository,
        IInventoryRepository inventoryRepository,
        IProductRepository productRepository,
        IUnitRepository unitRepository,
        UnitConversionService unitConversionService,
        IPaymentRepository paymentRepository,
        IVoucherRepository voucherRepository)
    {
        _uow = uow;
        _purchaseRepository = purchaseRepository;
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _unitConversionService = unitConversionService;
        _paymentRepository = paymentRepository;
        _voucherRepository = voucherRepository;
    }

    public async Task<long> CreatePurchaseAsync(Purchase purchase)
    {
        _uow.BeginTransaction();
        try
        {
            var purchaseId = await _purchaseRepository.CreateAsync(purchase);
            purchase.Id = purchaseId;

            // 3. Crear el Comprobante (Voucher) si aplica
            if (purchase.Voucher != null)
            {
                purchase.Voucher.PurchaseId = purchaseId;
                var voucherId = await _voucherRepository.CreateAsync(purchase.Voucher);
                await _purchaseRepository.UpdateVoucherIdAsync(purchaseId, voucherId);
            }

            // Foreach loop
            foreach (var detail in purchase.Details)
            {
                Unit? unit = null;
                if (detail.PurchaseUnitId.HasValue)
                {
                    unit = await _unitRepository.GetByIdAsync(detail.PurchaseUnitId.Value);
                }

                var baseQuantity = _unitConversionService.CalculateBaseQuantity(detail.Quantity, unit);

                // Purchases add stock to destination warehouse
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, purchase.WarehouseId, baseQuantity);

                // Update product cost
                await _productRepository.UpdateCostAsync(detail.ProductId, detail.Cost);
            }

            // Create payment record if paid
            /* if (purchase.PaidAmount > 0)
            {
                var paymentDto = new PaymentPurchaseDto
                {
                    UserId = purchase.UserId,
                    Date = purchase.Date,
                    Ref = "PAY-PUR-" + purchase.Ref,
                    PurchaseId = purchaseId,
                    Amount = purchase.PaidAmount,
                    Reglement = "Cash",
                    CreatedBy = purchase.CreatedBy
                };
                await _paymentRepository.CreatePurchasePaymentAsync(paymentDto);
            }*/

            _uow.Commit();
            return purchaseId;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
