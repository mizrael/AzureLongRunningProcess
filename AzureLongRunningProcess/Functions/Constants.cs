namespace AzureLongRunningProcess.Functions
{
    public sealed class RoutesParams
    {
        public const string ProcessId = "{processId}";
    }

    public static class Routes
    {
        public const string BaseProcess = "processes/";
        public const string ProcessDetails = Routes.BaseProcess + RoutesParams.ProcessId;
        public static string BuildProcessDetails(string id) => Routes.ProcessDetails.Replace(RoutesParams.ProcessId, id);
    }
}