using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

/// <summary>
/// Defines business operations related to Sales processing.
/// </summary>
public interface ISaleService
{
    /// <summary>
    /// Retrieves a paginated list of sales based on filter and sorting criteria.
    /// </summary>
    /// <param name="pagingParams">Pagination, filtering, and sorting parameters.</param>
    /// <returns>A paginated result of sales read-only DTOs.</returns>
    Task<PagedResult<SaleReadDto>> GetAllAsync(PagingParams pagingParams);

    /// <summary>
    /// Retrieves a sale by its unique identifier, including its details and associated voucher.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <returns>The sale entity if found; otherwise, null.</returns>
    Task<Sale?> GetByIdAsync(long id);

    /// <summary>
    /// Processes and creates a new sale transaction.
    /// This includes stock decrement, inventory verification, voucher issuance, and payment registration under a single transaction.
    /// </summary>
    /// <param name="sale">The sale entity containing transactional data and details.</param>
    /// <param name="userId">The unique identifier of the operator/cashier executing the sale.</param>
    /// <returns>The newly created sale's unique identifier.</returns>
    Task<long> CreateSaleAsync(Sale sale, long userId);

    /// <summary>
    /// Reverts and soft-deletes a sale transaction, restoring stock values to the warehouse if valid.
    /// Cannot delete sales belonging to closed cash shifts.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete.</param>
    /// <param name="userId">The unique identifier of the operator performing the deletion.</param>
    /// <returns>True if the sale was successfully deleted; otherwise, false.</returns>
    Task<bool> DeleteSaleAsync(long id, long userId);

    /// <summary>
    /// Creates a pending online sale.
    /// </summary>
    /// <param name="sale">The sale containing online checkout details.</param>
    /// <returns>The newly created online sale's ID.</returns>
    Task<long> CreateOnlineSaleAsync(Sale sale);

    /// <summary>
    /// Approves a pending online sale, shifting status to 'PROCESSING' and actualizing inventory stock.
    /// </summary>
    /// <param name="id">The unique identifier of the online sale.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    Task<bool> ApproveOnlineSaleAsync(long id);

    /// <summary>
    /// Verifies and approves a pending online sale. Sets status to 'PAID', registers the cashier operator, open cash shift, discounts stock, and triggers SIAT billing.
    /// </summary>
    Task<bool> VerifyOnlineSaleAsync(long id, long userId, long cashShiftId);

    /// <summary>
    /// Rejects/cancels an online sale, restoring inventory stock if the sale was already processed.
    /// </summary>
    /// <param name="id">The unique identifier of the online sale.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    Task<bool> RejectOnlineSaleAsync(long id);
}
