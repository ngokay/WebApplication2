using System.Diagnostics;

namespace WebApplication2.Interface
{
    public interface ITracingService
    {
        Task<T> TraceAsync<T>(string spanName, Func<Task<T>> action, Dictionary<string, object>? tags = null);
        Task TraceAsync(string spanName, Func<Task> action, Dictionary<string, object>? tags = null);
        T Trace<T>(string spanName, Func<T> action, Action<Activity>? enrich = null);
    }
}
