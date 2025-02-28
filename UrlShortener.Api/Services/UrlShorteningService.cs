using Dapper;
using Microsoft.Extensions.Caching.Hybrid;
using Npgsql;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

internal sealed class UrlshorteningService(
    NpgsqlDataSource dataSource,
    HybridCache cache,
    ILogger<UrlshorteningService> logger
)
{
    private const int MaxRetries = 3;

    public async Task<string> ShortenUrl(string originalUrl)
    {
        for (int attemps = 0; attemps <= MaxRetries; attemps++)
        {
            try
            {
                var shortCode = GenerateShortCode();

                const string sql = """
                    INSERT INTO shortened_urls (short_code, original_url)
                    VALUES (@ShortCode, @OriginalUrl)
                    RETURNING short_code;
                    """;

                await using var connection = await dataSource.OpenConnectionAsync();

                var result = await connection.QuerySingleAsync<string>(
                    sql,
                    new { ShortCode = shortCode, OriginalUrl = originalUrl }
                );

                await cache.SetAsync(shortCode, originalUrl);

                return result;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                if (attemps == MaxRetries)
                {
                    logger.LogError(
                        ex,
                        "Failed to generate unique short code after {MaxRetries} attempts",
                        MaxRetries
                    );

                    throw new InvalidOperationException("Failed to generate unique short code", ex);
                }

                logger.LogWarning("Shortcode collision detected, retrying...");
            }
        }

        throw new InvalidOperationException("Failed to generate unique short code");
    }

    private static string GenerateShortCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var shortCode = new char[7];

        for (var i = 0; i < shortCode.Length; i++)
        {
            shortCode[i] = chars[random.Next(chars.Length)];
        }

        return new string(shortCode);
    }

    public async Task<string> GetOriginalUrl(string shortCode)
    {
        var originalUrl = await cache.GetOrCreateAsync(
            shortCode,
            async token =>
            {
                const string sql = """
                SELECT original_url FROM shortened_urls WHERE short_code = @ShortCode;
                """;

                await using var connection = await dataSource.OpenConnectionAsync();

                return await connection.QuerySingleOrDefaultAsync<string>(
                    sql,
                    new { ShortCode = shortCode }
                );
            }
        );

        return originalUrl!;
    }

    public async Task<IEnumerable<ShortenedUrl>> GetAllUrls()
    {
        const string sql = """
            SELECT short_code as ShortCode, original_url as OriginalUrl, created_at as CreatedAt FROM shortened_urls;
            """;

        await using var connection = await dataSource.OpenConnectionAsync();

        return await connection.QueryAsync<ShortenedUrl>(sql);
    }
}
