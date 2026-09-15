using System.Data;
using Homebites.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Homebites.Data
{
    public static class DbInitializer
    {
        public static void Initialize(HomebitesDbContext context)
        {
            context.Database.EnsureCreated();
            EnsureUserColumns(context);
            EnsureOrderColumns(context);
        }

        private static void EnsureUserColumns(HomebitesDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen)
            {
                connection.Open();
            }

            try
            {
                var requiredColumns = new Dictionary<string, string>
                {
                    ["ProfileImageUrl"] = "NVARCHAR(500) NULL",
                    ["LastLoginAt"] = "DATETIME NULL",
                    ["PasswordChangedAt"] = "DATETIME NULL",
                    ["DeactivatedAt"] = "DATETIME NULL",
                    ["UpdatedAt"] = "DATETIME NULL"
                };

                foreach (var column in requiredColumns)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        IF COL_LENGTH('Users', @ColumnName) IS NULL
                            ALTER TABLE [Users] ADD [" + column.Key + @"] " + column.Value + @";";

                    var parameter = new SqlParameter("@ColumnName", column.Key);
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    connection.Close();
                }
            }
        }

        private static void EnsureOrderColumns(HomebitesDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen)
            {
                connection.Open();
            }

            try
            {
                var requiredColumns = new Dictionary<string, string>
                {
                    ["EstimatedTime"] = "DATETIME NULL",
                    ["CompletedAt"] = "DATETIME NULL"
                };

                foreach (var column in requiredColumns)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        IF COL_LENGTH('Orders', @ColumnName) IS NULL
                            ALTER TABLE [Orders] ADD [" + column.Key + @"] " + column.Value + @";";

                    var parameter = new SqlParameter("@ColumnName", column.Key);
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    connection.Close();
                }
            }
        }
    }
}