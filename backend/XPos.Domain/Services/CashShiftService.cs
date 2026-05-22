using System;
using System.Text;
using System.Threading.Tasks;
using XPos.Domain.Dtos;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class CashShiftService : ICashShiftService
{
    private readonly IUnitOfWork _uow;
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly ICashTransactionRepository _cashTransactionRepository;
    private readonly IUserRepository _userRepository;

    public CashShiftService(
        IUnitOfWork uow,
        ICashShiftRepository cashShiftRepository,
        ICashRegisterRepository cashRegisterRepository,
        ICashTransactionRepository cashTransactionRepository,
        IUserRepository userRepository)
    {
        _uow = uow;
        _cashShiftRepository = cashShiftRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _cashTransactionRepository = cashTransactionRepository;
        _userRepository = userRepository;
    }

    public async Task<long> OpenShiftAsync(long registerId, long userId, decimal startingCash, long activeWarehouseId)
    {
        // 1. Verify register exists and is active and belongs to the active warehouse
        var register = await _cashRegisterRepository.GetByIdAsync(registerId);
        if (register == null)
        {
            throw new InvalidOperationException("La caja especificada no existe.");
        }
        if (!register.IsActive)
        {
            throw new InvalidOperationException("La caja seleccionada está inactiva.");
        }
        if (register.WarehouseId != activeWarehouseId)
        {
            throw new InvalidOperationException("La caja no pertenece al almacén activo actual.");
        }

        // 2. Verify cashier does not already have an OPEN shift in the active warehouse
        var activeUserShift = await _cashShiftRepository.GetActiveShiftAsync(userId, activeWarehouseId);
        if (activeUserShift != null)
        {
            throw new InvalidOperationException("El usuario ya tiene un turno de caja abierto en este almacén.");
        }

        // 3. Verify cash register is not occupied (already has an OPEN shift)
        var activeRegisterShift = await _cashShiftRepository.GetActiveShiftByRegisterIdAsync(registerId);
        if (activeRegisterShift != null)
        {
            throw new InvalidOperationException("La caja seleccionada ya tiene un turno activo abierto por otro usuario.");
        }

        _uow.BeginTransaction();
        try
        {
            var shift = new CashShift
            {
                CashRegisterId = registerId,
                UserId = userId,
                Status = "OPEN",
                OpenedAt = DateTime.UtcNow,
                StartingCash = startingCash,
                EndingCashExpected = startingCash
            };

            var shiftId = await _cashShiftRepository.CreateAsync(shift);
            _uow.Commit();
            return shiftId;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<string> ExecuteManualTransactionAsync(long shiftId, string type, decimal amount, string notes, string recipientName, long userId)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("El monto debe ser mayor que cero.");
        }

        var shift = await _cashShiftRepository.GetByIdAsync(shiftId);
        if (shift == null)
        {
            throw new InvalidOperationException("El turno especificado no existe.");
        }
        if (shift.Status != "OPEN")
        {
            throw new InvalidOperationException("No se pueden registrar transacciones en un turno cerrado.");
        }

        _uow.BeginTransaction();
        try
        {
            // Calculate daily sequence incremental code
            var count = await _cashTransactionRepository.GetDailyTransactionCountAsync();
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var seqPart = (count + 1).ToString("D4");
            var voucherNumber = $"VOUCHER-CASH-{datePart}-{seqPart}";

            var transaction = new CashTransaction
            {
                CashShiftId = shiftId,
                VoucherNumber = voucherNumber,
                TransactionType = type.ToUpper(), // "FLOAT_IN", "FLOAT_OUT", "CASH_DROP", "EXPENSE"
                Amount = amount,
                Notes = notes,
                RecipientName = recipientName,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _cashTransactionRepository.CreateAsync(transaction);
            _uow.Commit();
            return voucherNumber;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> CloseShiftAsync(long shiftId, decimal actualCash, string notes)
    {
        var shift = await _cashShiftRepository.GetByIdAsync(shiftId);
        if (shift == null)
        {
            throw new InvalidOperationException("El turno especificado no existe.");
        }
        if (shift.Status != "OPEN")
        {
            throw new InvalidOperationException("El turno ya está cerrado.");
        }

        _uow.BeginTransaction();
        try
        {
            // Formula: EndingCashExpected = StartingCash + CashSales + FloatIns - CashDrops - Expenses
            var startingCash = shift.StartingCash;
            var cashSales = await _cashShiftRepository.GetCashSalesAmountAsync(shiftId);
            var floatIns = await _cashShiftRepository.GetManualTransactionsSumAsync(shiftId, "FLOAT_IN");
            var cashDrops = await _cashShiftRepository.GetManualTransactionsSumAsync(shiftId, "CASH_DROP");
            var expenses = await _cashShiftRepository.GetManualTransactionsSumAsync(shiftId, "EXPENSE");

            var expectedCash = startingCash + cashSales + floatIns - cashDrops - expenses;
            var discrepancy = actualCash - expectedCash;

            shift.Status = "CLOSED";
            shift.ClosedAt = DateTime.UtcNow;
            shift.EndingCashExpected = expectedCash;
            shift.EndingCashActual = actualCash;
            shift.Discrepancy = discrepancy;
            shift.ClosingNotes = notes;

            var success = await _cashShiftRepository.UpdateAsync(shift);
            _uow.Commit();
            return success;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<CashShiftReceiptDto?> GetReceiptPayloadAsync(long shiftId)
    {
        var shift = await _cashShiftRepository.GetByIdAsync(shiftId);
        if (shift == null) return null;

        var register = await _cashRegisterRepository.GetByIdAsync(shift.CashRegisterId);
        var registerName = register?.Name ?? "N/A";

        var user = await _userRepository.GetByIdAsync(shift.UserId);
        var cashierName = user != null ? $"{user.FirstName} {user.LastName} ({user.Username})" : "N/A";

        _uow.BeginTransaction();
        decimal cashSales, floatIns, cashDrops, expenses;
        try
        {
            cashSales = await _cashShiftRepository.GetCashSalesAmountAsync(shiftId);
            floatIns = await _cashShiftRepository.GetManualTransactionsSumAsync(shiftId, "FLOAT_IN");
            cashDrops = await _cashShiftRepository.GetManualTransactionsSumAsync(shiftId, "CASH_DROP");
            expenses = await _cashShiftRepository.GetManualTransactionsSumAsync(shiftId, "EXPENSE");
            _uow.Commit();
        }
        catch
        {
            _uow.Rollback();
            throw;
        }

        var startingCash = shift.StartingCash;
        var expectedCash = startingCash + cashSales + floatIns - cashDrops - expenses;
        var actualCash = shift.EndingCashActual ?? 0;
        var discrepancy = shift.Discrepancy ?? 0;

        // Generate formatted thermal print receipt text (80mm width / 48 characters)
        var sb = new StringBuilder();
        sb.AppendLine("================================================");
        sb.AppendLine("               TICKET DE ARQUEO                 ");
        sb.AppendLine("================================================");
        sb.AppendLine(FormatLine("Turno ID:", $"#{shift.Id}"));
        sb.AppendLine(FormatLine("Caja:", registerName));
        sb.AppendLine(FormatLine("Cajero:", cashierName));
        sb.AppendLine(FormatLine("Apertura:", shift.OpenedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")));
        sb.AppendLine(FormatLine("Cierre:", shift.ClosedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"));
        sb.AppendLine("------------------------------------------------");
        sb.AppendLine(FormatLine("Efectivo Inicial (+):", startingCash.ToString("F2")));
        sb.AppendLine(FormatLine("Ventas Efectivo (+):", cashSales.ToString("F2")));
        sb.AppendLine(FormatLine("Ingresos Manuales (+):", floatIns.ToString("F2")));
        sb.AppendLine(FormatLine("Remesas/Drops (-):", cashDrops.ToString("F2")));
        sb.AppendLine(FormatLine("Egresos/Gastos (-):", expenses.ToString("F2")));
        sb.AppendLine("------------------------------------------------");
        sb.AppendLine(FormatLine("Saldo Esperado:", expectedCash.ToString("F2")));
        sb.AppendLine(FormatLine("Saldo Real Arqueo:", shift.EndingCashActual?.ToString("F2") ?? "N/A"));
        sb.AppendLine(FormatLine("Discrepancia:", discrepancy.ToString("F2")));
        sb.AppendLine("------------------------------------------------");
        if (!string.IsNullOrEmpty(shift.ClosingNotes))
        {
            sb.AppendLine("Notas de Cierre:");
            sb.AppendLine(shift.ClosingNotes);
            sb.AppendLine("------------------------------------------------");
        }
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("      _________________       _________________ ");
        sb.AppendLine("       Firma del Cajero        Firma Receptor   ");
        sb.AppendLine();

        return new CashShiftReceiptDto
        {
            ShiftId = shiftId,
            CashRegisterName = registerName,
            CashierName = cashierName,
            OpenedAt = shift.OpenedAt,
            ClosedAt = shift.ClosedAt,
            StartingCash = startingCash,
            CashSales = cashSales,
            FloatIns = floatIns,
            CashDrops = cashDrops,
            Expenses = expenses,
            EndingCashExpected = expectedCash,
            EndingCashActual = actualCash,
            Discrepancy = discrepancy,
            ClosingNotes = shift.ClosingNotes,
            FormattedText = sb.ToString()
        };
    }

    public async Task<ActiveShiftDto?> GetActiveShiftAsync(long userId, long warehouseId)
    {
        var shift = await _cashShiftRepository.GetActiveShiftAsync(userId, warehouseId);
        if (shift == null) return null;

        var register = await _cashRegisterRepository.GetByIdAsync(shift.CashRegisterId);
        var registerName = register?.Name ?? "N/A";

        var user = await _userRepository.GetByIdAsync(shift.UserId);
        var username = user?.Username ?? "N/A";

        return new ActiveShiftDto
        {
            ShiftId = shift.Id,
            RegisterId = shift.CashRegisterId,
            RegisterName = registerName,
            UserId = shift.UserId,
            Username = username,
            Status = shift.Status,
            OpenedAt = shift.OpenedAt,
            StartingCash = shift.StartingCash
        };
    }

    private string FormatLine(string left, string right, int width = 48)
    {
        if (left.Length + right.Length >= width)
        {
            return left + " " + right;
        }
        int spaces = width - left.Length - right.Length;
        return left + new string(' ', spaces) + right;
    }
}
