using Microsoft.EntityFrameworkCore.Migrations;

namespace BookingHub.Infrastructure.Persistence;

/// <summary>
/// The database objects EF Core does not model — the booking overlap constraint, the unique phone
/// index, row-level security and the application role. They live as SQL scripts in Persistence/Sql
/// (embedded in this assembly) and are applied by the InitialCreate migration:
/// <see cref="Create"/> at the end of Up, <see cref="Drop"/> at the start of Down.
/// </summary>
internal static class DatabaseObjects
{
    /// <summary>In dependency order; dropped in reverse.</summary>
    private static readonly string[] Scripts =
    [
        "01-booking-overlap",
        "02-client-phone",
        "03-row-level-security",
        "04-application-role"
    ];

    public static void Create(MigrationBuilder migrationBuilder)
    {
        foreach (var script in Scripts)
            migrationBuilder.Sql(Read($"{script}.up.sql"));
    }

    public static void Drop(MigrationBuilder migrationBuilder)
    {
        foreach (var script in Scripts.Reverse())
            migrationBuilder.Sql(Read($"{script}.down.sql"));
    }

    private static string Read(string fileName)
    {
        var resourceName = $"Sql/{fileName}";
        using var stream = typeof(DatabaseObjects).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded SQL script {resourceName} is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}