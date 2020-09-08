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
    public interface ILongRunningProcessOrchestrator
    {
        void Start(StartOperation command);
        void OnCompleted();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LongRunningProcessOrchestrator : ILongRunningProcessOrchestrator
    {
        private readonly IDurableEntityContext _context;
        private readonly ILogger<LongRunningProcessOrchestrator> _logger;

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProcessStatus Status { get; private set; }

        [JsonProperty("id")]
        public Guid Id { get; private set; }

        public LongRunningProcessOrchestrator(IDurableEntityContext context, ILogger<LongRunningProcessOrchestrator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Start(StartOperation command)
        {
            Id = command.RequestId;
            Status = ProcessStatus.Started;

            _logger.LogInformation($"starting new process {Id} ...");

            var runnerId = new EntityId(nameof(LongRunningProcessRunner), command.RequestId.ToString());
            _context.SignalEntity<ILongRunningProcessRunner>(runnerId, r => r.RunAsync(command));
        }

        public void OnCompleted()
        {
            Status = ProcessStatus.Completed;

            _logger.LogInformation($"process {Id} completed!");
        }

        [FunctionName(nameof(LongRunningProcessOrchestrator))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<LongRunningProcessOrchestrator>(ctx);
    }
}