using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IAdjustmentService
{
    Task<long> CreateAdjustmentAsync(Adjustment adjustment);
}
