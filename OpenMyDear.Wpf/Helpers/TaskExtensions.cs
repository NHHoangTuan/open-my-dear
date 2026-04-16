namespace OpenMyDear.Wpf.Helpers;

public static class TaskExtensions
{
    public static async void SafeFireAndForget(this Task task, Action<Exception>? onError = null)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
        }
    }
}
