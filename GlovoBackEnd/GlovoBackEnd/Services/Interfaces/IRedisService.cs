namespace GlovoBackEnd.Services.Interfaces;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T data, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}