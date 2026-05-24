using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICmsRepository
{
    Task<IEnumerable<CmsPage>> GetAllAsync();
    Task<CmsPage?> GetByIdAsync(long id);
    Task<CmsPage?> GetBySlugAsync(string slug);
    Task<long> CreateAsync(CmsPage page);
    Task<bool> UpdateAsync(CmsPage page);
    Task<bool> DeleteAsync(long id, long? deletedBy);
}
