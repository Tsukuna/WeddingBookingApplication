using Microsoft.EntityFrameworkCore;
using WeddingBookingApplication.Database.AppDbContextModels;

namespace WeddingBookingApplication.WindowForm.Helpers;

/// <summary>
/// Provides a configured <see cref="AppDbContext"/> for the WinForms layer.
/// The connection string mirrors the one in <c>AppDbContext.OnConfiguring</c>.
/// </summary>
public static class DbContextFactory
{
    private const string ConnectionString =
        "Server=LAPTOP-1LF20QJ8\\SQLEXPRESS;Database=WeddingBookingDb;" +
        "User ID=sa;Password=sasa@123;TrustServerCertificate=True;";

    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}
