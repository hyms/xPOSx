using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IPaymentGatewaySettingsRepository
{
    Task<IEnumerable<PaymentGatewaySettings>> GetAllAsync();
    Task<PaymentGatewaySettings?> GetByIdAsync(long id);
    Task<long> CreateAsync(PaymentGatewaySettings settings);
    Task<bool> UpdateAsync(PaymentGatewaySettings settings);
    Task<bool> DeleteAsync(long id);
}
