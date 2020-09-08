using System;
using System.Threading.Tasks;
using AzureLongRunningProcess.Commands;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureLongRunningProcess.Functions.Entities
{
    public interface ILongRunningProcessRunner
    {
        Task RunAsync(StartOperation command);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LongRunningProcessRunner : ILongRunningProcessRunner
    {
        private readonly IDurableEntityContext _context;

        private readonly ILogger<LongRunningProcessOrchestrator> _logger;

        public LongRunningProcessRunner(IDurableEntityContext context, ILogger<LongRunningProcessOrchestrator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RunAsync(StartOperation command)
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);
            var delay = TimeSpan.FromSeconds(rand.Next(60));

            _logger.LogInformation($"starting long running process {command.RequestId} for {delay} ...");

            await Task.Delay(delay);

            var orchestratorId = new EntityId(nameof(LongRunningProcessOrchestrator), command.RequestId.ToString());
            _context.SignalEntity<ILongRunningProcessOrchestrator>(orchestratorId, r => r.OnCompleted());
        }

        [FunctionName(nameof(LongRunningProcessRunner))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<LongRunningProcessRunner>(ctx);
    }
}