using XPos.Domain.Dtos;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IQuotationService
{
    Task<IEnumerable<QuotationReadDto>> GetAllAsync();
    Task<Quotation?> GetByIdAsync(long id);
    Task<long> CreateAsync(CreateQuotationDto dto, long userId);
    Task<bool> UpdateAsync(UpdateQuotationDto dto, long userId);
    Task<bool> DeleteAsync(long id, long userId);
}
