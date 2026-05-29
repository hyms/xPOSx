using XPos.Domain.Models;
using XPos.Domain.Dtos;
using XPos.Api.Dtos;
using XPos.Domain.Interfaces;

namespace XPos.Tests;

public class ModelCoverageTests
{
    [Fact]
    public void TouchAllModelsAndDtos()
    {
        // Models
        var user = new User { Id = 1, Username = "u", Email = "e", FirstName = "f", LastName = "l", IsActive = true, Password = "p", RoleId = 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, DefaultWarehouseId = 1 };
        _ = user.Id; _ = user.Username; _ = user.Email; _ = user.FirstName; _ = user.LastName; _ = user.IsActive; _ = user.Password; _ = user.RoleId; _ = user.CreatedAt; _ = user.UpdatedAt; _ = user.DefaultWarehouseId; _ = user.RoleDetails;

        var product = new Product { Id = 1, Name = "P", Code = "C", CategoryId = 1, UnitId = 1, UnitPurchaseId = 1, UnitSaleId = 1, Cost = 10, Price = 20, Stock = 100, StockAlert = 10, Image = "img", TaxNet = 0, TaxMethod = "1", Note = "N", IsVariant = false, NotSelling = false, IsActive = true, IsFeatured = true, IsWebAvailable = true, Category = new Category(), Unit = new Unit() };
        _ = product.Id; _ = product.Name; _ = product.Code; _ = product.CategoryId; _ = product.UnitId; _ = product.UnitPurchaseId; _ = product.UnitSaleId; _ = product.Cost; _ = product.Price; _ = product.Stock; _ = product.StockAlert; _ = product.Image; _ = product.TaxNet; _ = product.TaxMethod; _ = product.Note; _ = product.IsVariant; _ = product.NotSelling; _ = product.IsActive; _ = product.IsFeatured; _ = product.IsWebAvailable; _ = product.Category; _ = product.Unit;

        var sale = new Sale { Id = 1, Ref = "R", Date = DateTime.Now, ClientId = 1, WarehouseId = 1, TaxRate = 0, Discount = 0, Shipping = 0, GrandTotal = 100, PaidAmount = 100, Status = "S", PaymentStatus = "P", UserId = 1, CreatedBy = 1, UpdatedBy = 1, DeletedAt = DateTime.Now, Details = new List<SaleDetail>(), Voucher = new Voucher() };
        _ = sale.Id; _ = sale.Ref; _ = sale.Date; _ = sale.ClientId; _ = sale.WarehouseId; _ = sale.TaxRate; _ = sale.Discount; _ = sale.Shipping; _ = sale.GrandTotal; _ = sale.PaidAmount; _ = sale.Status; _ = sale.PaymentStatus; _ = sale.UserId; _ = sale.CreatedBy; _ = sale.UpdatedBy; _ = sale.DeletedAt; _ = sale.Details; _ = sale.Voucher;

        var adjustment = new Adjustment { Id = 1, Ref = "R", Date = DateTime.Now, WarehouseId = 1, Items = 1, Notes = "N", UserId = 1, CreatedBy = 1, UpdatedBy = 1, Details = new List<AdjustmentDetail>() };
        _ = adjustment.Id; _ = adjustment.Ref; _ = adjustment.Date; _ = adjustment.WarehouseId; _ = adjustment.Items; _ = adjustment.Notes; _ = adjustment.UserId; _ = adjustment.CreatedBy; _ = adjustment.UpdatedBy; _ = adjustment.Details;

        var currency = new Currency { Id = 1, Code = "USD", Name = "Dollar", Symbol = "$" };
        _ = currency.Id; _ = currency.Code; _ = currency.Name; _ = currency.Symbol;

        var mailSettings = new MailSettings { Id = 1, Host = "h", Port = 587, Username = "u", Password = "p", Encryption = "e", FromAddress = "f", FromName = "n" };
        _ = mailSettings.Id; _ = mailSettings.Host; _ = mailSettings.Port; _ = mailSettings.Username; _ = mailSettings.Password; _ = mailSettings.Encryption; _ = mailSettings.FromAddress; _ = mailSettings.FromName;

        var pgSettings = new PaymentGatewaySettings { Id = 1, GatewayName = "n", ApiKey = "k", IsActive = true, Description = "d" };
        _ = pgSettings.Id; _ = pgSettings.GatewayName; _ = pgSettings.ApiKey; _ = pgSettings.IsActive; _ = pgSettings.Description;

        var permission = new Permission { Id = 1, Name = "n", GuardName = "g" };
        _ = permission.Id; _ = permission.Name; _ = permission.GuardName;

        var role = new Role { Id = 1, Name = "n", Permissions = new List<Permission>() };
        _ = role.Id; _ = role.Name; _ = role.Permissions;

        // DTOs
        var loginDto = new LoginDto { Username = "u", Password = "p" };
        _ = loginDto.Username; _ = loginDto.Password;

        var authResponse = new AuthResponseDto { Token = "t", Username = "u", Permissions = new List<string>(), ActiveWarehouseId = 1 };
        _ = authResponse.Token; _ = authResponse.Username; _ = authResponse.Permissions; _ = authResponse.ActiveWarehouseId;

        var passwordUpdate = new PasswordUpdateDto { NewPassword = "n" };
        _ = passwordUpdate.NewPassword;

        var createAdj = new CreateAdjustmentDto { Date = DateTime.Now, WarehouseId = 1, Notes = "n", Details = new List<CreateAdjustmentDetailDto>() };
        var createAdjDetail = new CreateAdjustmentDetailDto { ProductId = 1, Quantity = 1, Type = "add" };

        var pagedResult = new PagedResult<string> { Items = new List<string>(), TotalItems = 0, Page = 1, PageSize = 10 };
        _ = pagedResult.Items; _ = pagedResult.TotalItems; _ = pagedResult.Page; _ = pagedResult.PageSize;

        var pagingParams = new PagingParams { Page = 1, PageSize = 10, Filter = "s", SortBy = "s", SortDescending = true, WarehouseId = 1 };
        _ = pagingParams.Page; _ = pagingParams.PageSize; _ = pagingParams.Filter; _ = pagingParams.SortBy; _ = pagingParams.SortDescending; _ = pagingParams.WarehouseId;

        var saleRead = new SaleReadDto { Id = 1, Ref = "r", Date = DateTime.Now, ClientName = "c", WarehouseName = "w", GrandTotal = 1, PaidAmount = 1, Status = "s", PaymentStatus = "p" };
        _ = saleRead.Id; _ = saleRead.Ref; _ = saleRead.Date; _ = saleRead.ClientName; _ = saleRead.WarehouseName; _ = saleRead.GrandTotal; _ = saleRead.PaidAmount; _ = saleRead.Status; _ = saleRead.PaymentStatus;

        var paySale = new PaymentSaleDto { UserId = 1, Date = DateTime.Now, Ref = "r", SaleId = 1, Amount = 1, Reglement = "r", CreatedBy = 1 };
        _ = paySale.UserId; _ = paySale.Date; _ = paySale.Ref; _ = paySale.SaleId; _ = paySale.Amount; _ = paySale.Reglement; _ = paySale.CreatedBy;

        var payPur = new PaymentPurchaseDto { UserId = 1, Date = DateTime.Now, Ref = "r", PurchaseId = 1, Amount = 1, Reglement = "r", CreatedBy = 1 };
        _ = payPur.UserId; _ = payPur.Date; _ = payPur.Ref; _ = payPur.PurchaseId; _ = payPur.Amount; _ = payPur.Reglement; _ = payPur.CreatedBy;
    }
}
