using System.Text.Json;
using StackExchange.Redis;
using TwoFactorAuthAPI.Models;

public class CodeCleanupService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;

    public CodeCleanupService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _redis = redis;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            CleanExpiredCodes();
            await Task.Delay(TimeSpan.FromMinutes(4), stoppingToken); // Run cleanup every minute
        }
    }

    private void CleanExpiredCodes()
    {
        var currentTime = DateTime.UtcNow;
        var db = _redis.GetDatabase();
       
        // Retrieve all keys in Redis (assuming all keys store phone codes)
        RedisValue[] redisValues = db.ListRange("*");

        // Convert RedisValue array to a list
        var keys = redisValues.Select(x => (string)x).ToList();

        foreach (var key in keys)
        {
            var phoneCodesJson = db.StringGet(key);

            if (!phoneCodesJson.IsNullOrEmpty)
            {
                var phoneCodes = JsonSerializer.Deserialize<List<ConfirmationCode>>(phoneCodesJson);

                if (phoneCodes != null)
                {
                    phoneCodes.RemoveAll(code =>
                        currentTime.Subtract(code.CreationTime).TotalMinutes >
                        _configuration.GetValue<int>("CodeLifetimeMinutes"));

                    // Update Redis with cleaned codes
                    db.StringSet(key, JsonSerializer.Serialize(phoneCodes));
                }
            }
        }
    }
}
