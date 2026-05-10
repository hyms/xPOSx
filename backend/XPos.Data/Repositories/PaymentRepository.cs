using Dapper;
using XPos.Domain.Interfaces;

namespace XPos.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IUnitOfWork _uow;

    public PaymentRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<long> CreateSalePaymentAsync(PaymentSaleDto payment)
    {
        const string paymentSql = @"
            INSERT INTO payment_sales (user_id, date, ref, sale_id, amount, reglement, created_at, created_by)
            VALUES (@UserId, @Date, @Ref, @SaleId, @Amount, @Reglement, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(paymentSql, payment, _uow.Transaction);
    }

    public async Task<long> CreatePurchasePaymentAsync(PaymentPurchaseDto payment)
    {
        const string paymentSql = @"
            INSERT INTO payment_purchases (user_id, date, ref, purchase_id, amount, reglement, created_at, created_by)
            VALUES (@UserId, @Date, @Ref, @PurchaseId, @Amount, @Reglement, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(paymentSql, payment, _uow.Transaction);
    }
}
