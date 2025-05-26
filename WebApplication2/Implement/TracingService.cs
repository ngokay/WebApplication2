namespace WebApplication2.Implement
{
    using System.Diagnostics;
    using WebApplication2.Interface;

    public class TracingService : ITracingService
    {
        private static readonly ActivitySource ActivitySource = new("my-dotnet-service");

        public async Task<T> TraceAsync<T>(string spanName, Func<Task<T>> action, Dictionary<string, object>? tags = null)
        {
            using var activity = ActivitySource.StartActivity(spanName, ActivityKind.Internal);

            if (tags != null)
            {
                foreach (var tag in tags)
                    activity?.SetTag(tag.Key, tag.Value);
            }

            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception", ex.ToString());
                throw;
            }
        }

        public async Task TraceAsync(string spanName, Func<Task> action, Dictionary<string, object>? tags = null)
        {
            using var activity = ActivitySource.StartActivity(spanName, ActivityKind.Internal);

            if (tags != null)
            {
                foreach (var tag in tags)
                    activity?.SetTag(tag.Key, tag.Value);
            }

            try
            {
                await action();
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception", ex.ToString());
                throw;
            }
        }

        public T Trace<T>(string spanName, Func<T> action, Action<Activity>? enrich = null)
        {
            using var activity = ActivitySource.StartActivity(spanName);
            enrich?.Invoke(activity!);
            return action();
        }
    }

}
