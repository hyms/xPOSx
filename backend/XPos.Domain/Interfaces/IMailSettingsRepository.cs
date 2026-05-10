using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IMailSettingsRepository
{
    Task<MailSettings?> GetAsync(); // Assuming only one set of mail settings
    Task<long> CreateAsync(MailSettings mailSettings);
    Task<bool> UpdateAsync(MailSettings mailSettings);
}
