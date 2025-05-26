namespace WebApplication2.Interface
{
    public interface IRedisService
    {
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<bool> DeleteAsync(string key);
    }
}
