using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MiniBusiness.Infrastructure.Extensions;

public static class PostgresExceptionExtensions
{
    public static bool IsUniqueViolation(this Exception ex)
    {
        return ex is DbUpdateException dbEx 
               && dbEx.InnerException is PostgresException pgEx 
               && pgEx.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}