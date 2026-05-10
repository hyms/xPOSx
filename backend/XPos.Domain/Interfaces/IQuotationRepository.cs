using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IQuotationRepository
{
    Task<IEnumerable<QuotationReadDto>> GetAllAsync();
    Task<Quotation?> GetByIdAsync(long id);
    Task<long> CreateAsync(Quotation quotation);
    Task<bool> UpdateAsync(Quotation quotation);
    Task<bool> DeleteAsync(long id, long userId);
}
