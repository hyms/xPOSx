using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IMailSettingsService
{
    Task<MailSettings?> GetAsync();
    Task<long> CreateOrUpdateAsync(MailSettings mailSettings);
}
