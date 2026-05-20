using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IClientRepository
{
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(long id);
    Task<long> CreateAsync(Client client);
    Task<Client?> GetByNitAsync(string nit);
    Task<bool> UpdateAsync(Client client);
    Task<bool> DeleteAsync(long id);
}

public interface IProviderRepository
{
    Task<IEnumerable<Provider>> GetAllAsync();
    Task<Provider?> GetByIdAsync(long id);
    Task<long> CreateAsync(Provider provider);
    Task<bool> UpdateAsync(Provider provider);
    Task<bool> DeleteAsync(long id);
}
