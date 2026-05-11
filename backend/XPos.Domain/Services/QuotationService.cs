using XPos.Domain.Dtos;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class QuotationService : IQuotationService
{
    private readonly IQuotationRepository _repository;

    public QuotationService(IQuotationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<QuotationReadDto>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Quotation?> GetByIdAsync(long id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<long> CreateAsync(CreateQuotationDto dto, long userId)
    {
        var quotation = new Quotation
        {
            Date = dto.Date,
            ClientId = dto.ClientId,
            WarehouseId = dto.WarehouseId,
            TaxRate = dto.TaxRate,
            Discount = dto.Discount,
            Shipping = dto.Shipping,
            Status = dto.Status,
            Notes = dto.Notes,
            UserId = userId,
            CreatedBy = userId,
            Ref = $"QUO-{DateTime.Now:yyyyMMddHHmmss}",
            Details = dto.Details.Select(d => new QuotationDetail
            {
                ProductId = d.ProductId,
                Price = d.Price,
                Quantity = d.Quantity,
                Total = d.Price * d.Quantity
            }).ToList()
        };

        quotation.GrandTotal = quotation.Details.Sum(d => d.Total) - (quotation.Discount ?? 0) + (quotation.Shipping ?? 0);

        return await _repository.CreateAsync(quotation);
    }

    public async Task<bool> UpdateAsync(UpdateQuotationDto dto, long userId)
    {
        var quotation = new Quotation
        {
            Id = dto.Id,
            Date = dto.Date,
            ClientId = dto.ClientId,
            WarehouseId = dto.WarehouseId,
            TaxRate = dto.TaxRate,
            Discount = dto.Discount,
            Shipping = dto.Shipping,
            Status = dto.Status,
            Notes = dto.Notes,
            UpdatedBy = userId,
            Details = dto.Details.Select(d => new QuotationDetail
            {
                QuotationId = dto.Id,
                ProductId = d.ProductId,
                Price = d.Price,
                Quantity = d.Quantity,
                Total = d.Price * d.Quantity
            }).ToList()
        };

        quotation.GrandTotal = quotation.Details.Sum(d => d.Total) - (quotation.Discount ?? 0) + (quotation.Shipping ?? 0);

        return await _repository.UpdateAsync(quotation);
    }

    public async Task<bool> DeleteAsync(long id, long userId)
    {
        return await _repository.DeleteAsync(id, userId);
    }
}
