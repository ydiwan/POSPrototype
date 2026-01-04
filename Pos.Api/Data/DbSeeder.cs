using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pos.Api.Models;

namespace Pos.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(PosDbContext db)
        {
            EnsureLocationPosDisabledColumn(db);

            // Locations
            if (!db.Locations.Any())
            {
                db.Locations.AddRange(
                    new Location { Code = "MAIN", Name = "Main Store", IsPosDisabled = false },
                    new Location { Code = "STORE2", Name = "Second Store", IsPosDisabled = false }
                );
                db.SaveChanges();
            }

            // Products
            if (!db.Products.Any())
            {
                db.Products.AddRange(
                    new Product
                    {
                        Sku = "COFFEE_SM",
                        Name = "Coffee",
                        OptionGroup = "Size",
                        OptionValue = "Small",
                        Price = 2.50m,
                        IsActive = true
                    },
                    new Product
                    {
                        Sku = "COFFEE_LG",
                        Name = "Coffee",
                        OptionGroup = "Size",
                        OptionValue = "Large",
                        Price = 3.50m,
                        IsActive = true
                    },
                    new Product
                    {
                        Sku = "PASTRY",
                        Name = "Pastry",
                        OptionGroup = null,
                        OptionValue = null,
                        Price = 4.00m,
                        IsActive = true
                    }
                );
                db.SaveChanges();
            }

        }

        private static void EnsureLocationPosDisabledColumn(PosDbContext db)
        {
            try
            {
                db.Database.ExecuteSqlRaw(
                    "ALTER TABLE Locations ADD COLUMN IsPosDisabled INTEGER NOT NULL DEFAULT 0;");
            }
            catch (SqliteException ex) when (
                ex.SqliteErrorCode == 1 &&
                ex.Message.Contains("duplicate column", System.StringComparison.OrdinalIgnoreCase))
            {
                // Column already exists.
            }
        }
    }
}

