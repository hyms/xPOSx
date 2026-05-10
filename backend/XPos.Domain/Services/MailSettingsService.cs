using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class MailSettingsService : IMailSettingsService
{
    private readonly IUnitOfWork _uow;
    private readonly IMailSettingsRepository _mailSettingsRepository;

    public MailSettingsService(IUnitOfWork uow, IMailSettingsRepository mailSettingsRepository)
    {
        _uow = uow;
        _mailSettingsRepository = mailSettingsRepository;
    }

    public async Task<MailSettings?> GetAsync()
    {
        return await _mailSettingsRepository.GetAsync();
    }

    public async Task<long> CreateOrUpdateAsync(MailSettings mailSettings)
    {
        _uow.BeginTransaction();
        try
        {
            var existingSettings = await _mailSettingsRepository.GetAsync();
            long id;
            if (existingSettings == null)
            {
                id = await _mailSettingsRepository.CreateAsync(mailSettings);
            }
            else
            {
                mailSettings.Id = existingSettings.Id;
                await _mailSettingsRepository.UpdateAsync(mailSettings);
                id = existingSettings.Id;
            }
            _uow.Commit();
            return id;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
