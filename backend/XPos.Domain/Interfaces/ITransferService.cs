using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ITransferService
{
    Task<long> CreateTransferAsync(Transfer transfer);
}
