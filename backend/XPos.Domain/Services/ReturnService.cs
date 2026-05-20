using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

using XPos.Domain.Dtos;

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
    private readonly IPaymentRepository _paymentRepository;

    public ReturnService(
        IUnitOfWork uow,
        ISaleReturnRepository saleReturnRepository,
        IPurchaseReturnRepository purchaseReturnRepository,
        IInventoryRepository inventoryRepository,
        IUnitRepository unitRepository,
        UnitConversionService unitConversionService,
        IVoucherRepository voucherRepository,
        IPaymentRepository paymentRepository)
    {
        _uow = uow;
        _saleReturnRepository = saleReturnRepository;
        _purchaseReturnRepository = purchaseReturnRepository;
        _inventoryRepository = inventoryRepository;
        _unitRepository = unitRepository;
        _unitConversionService = unitConversionService;
        _voucherRepository = voucherRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<IEnumerable<SaleReturnReadDto>> GetAllSaleReturnsAsync()
    {
        return await _saleReturnRepository.GetAllAsync();
    }

    public async Task<SaleReturn?> GetSaleReturnByIdAsync(long id)
    {
        return await _saleReturnRepository.GetByIdAsync(id);
    }

    public async Task<long> CreateSaleReturnAsync(CreateSaleReturnDto dto, long userId)
    {
        var saleReturn = new SaleReturn
        {
            Date = dto.Date,
            ClientId = dto.ClientId,
            WarehouseId = dto.WarehouseId,
            GrandTotal = dto.GrandTotal,
            PaidAmount = dto.PaidAmount,
            Status = dto.Status,
            PaymentStatus = dto.PaymentStatus,
            UserId = userId,
            CreatedBy = userId,
            Ref = $"SR-{DateTime.Now:yyyyMMddHHmmss}",
            Details = dto.Details.Select(d => new SaleReturnDetail
            {
                ProductId = d.ProductId,
                Price = d.Price,
                Quantity = d.Quantity,
                Total = d.Price * d.Quantity
            }).ToList()
        };

        _uow.BeginTransaction();
        try
        {
            var returnId = await _saleReturnRepository.CreateAsync(saleReturn);
            saleReturn.Id = returnId;

            foreach (var detail in saleReturn.Details)
            {
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, saleReturn.WarehouseId, detail.Quantity);
            }

            if (saleReturn.PaidAmount > 0)
            {
                var paymentDto = new PaymentSaleDto
                {
                    UserId = saleReturn.UserId,
                    Date = saleReturn.Date,
                    Ref = "PAY-" + saleReturn.Ref,
                    SaleId = saleReturn.Id, 
                    Amount = -saleReturn.PaidAmount,
                    Reglement = "Cash",
                    CreatedBy = saleReturn.CreatedBy
                };
                await _paymentRepository.CreateSalePaymentAsync(paymentDto);
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

    public async Task<bool> DeleteSaleReturnAsync(long id, long userId)
    {
        return await _saleReturnRepository.DeleteAsync(id, userId);
    }

    public async Task<IEnumerable<PurchaseReturnReadDto>> GetAllPurchaseReturnsAsync()
    {
        return await _purchaseReturnRepository.GetAllAsync();
    }

    public async Task<PurchaseReturn?> GetPurchaseReturnByIdAsync(long id)
    {
        return await _purchaseReturnRepository.GetByIdAsync(id);
    }

    public async Task<long> CreatePurchaseReturnAsync(CreatePurchaseReturnDto dto, long userId)
    {
        var purchaseReturn = new PurchaseReturn
        {
            Date = dto.Date,
            ProviderId = dto.ProviderId,
            WarehouseId = dto.WarehouseId,
            GrandTotal = dto.GrandTotal,
            PaidAmount = dto.PaidAmount,
            Status = dto.Status,
            PaymentStatus = dto.PaymentStatus,
            UserId = userId,
            CreatedBy = userId,
            Ref = $"PR-{DateTime.Now:yyyyMMddHHmmss}",
            Details = dto.Details.Select(d => new PurchaseReturnDetail
            {
                ProductId = d.ProductId,
                Cost = d.Cost,
                Quantity = d.Quantity,
                Total = d.Cost * d.Quantity
            }).ToList()
        };

        _uow.BeginTransaction();
        try
        {
            var returnId = await _purchaseReturnRepository.CreateAsync(purchaseReturn);
            purchaseReturn.Id = returnId;

            foreach (var detail in purchaseReturn.Details)
            {
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, purchaseReturn.WarehouseId, -detail.Quantity);
            }

            if (purchaseReturn.PaidAmount > 0)
            {
                var paymentDto = new PaymentPurchaseDto
                {
                    UserId = purchaseReturn.UserId,
                    Date = purchaseReturn.Date,
                    Ref = "PAY-" + purchaseReturn.Ref,
                    PurchaseId = purchaseReturn.Id,
                    Amount = -purchaseReturn.PaidAmount,
                    Reglement = "Cash",
                    CreatedBy = purchaseReturn.CreatedBy
                };
                await _paymentRepository.CreatePurchasePaymentAsync(paymentDto);
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

    public async Task<bool> DeletePurchaseReturnAsync(long id, long userId)
    {
        return await _purchaseReturnRepository.DeleteAsync(id, userId);
    }
}
