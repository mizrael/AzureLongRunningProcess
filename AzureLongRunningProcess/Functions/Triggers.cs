using System;
using System.Threading.Tasks;
using AzureLongRunningProcess.Commands;
using AzureLongRunningProcess.Functions.Entities;
using AzureLongRunningProcess.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace AzureLongRunningProcess.Functions
{
    public static class Triggers
    {
        private const string QueueName = "long-running-requests";
        private const string QueueConnectionName = "AzureWebJobsStorage";

        [FunctionName("RequestProcess")]
        public static async Task<IActionResult> RequestProcess(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Routes.BaseProcess)] HttpRequest req,
            [Queue(QueueName, Connection = QueueConnectionName)] CloudQueue encryptionRequestsQueue)
        {
            var command = new StartOperation(Guid.NewGuid());

            var jsonMessage = System.Text.Json.JsonSerializer.Serialize(command);
            await encryptionRequestsQueue.AddMessageAsync(new CloudQueueMessage(jsonMessage));

            return new AcceptedObjectResult(Routes.BuildProcessDetails(command.RequestId.ToString()), command);
        }

        [FunctionName("GetStatus")]
        public static async Task<IActionResult> GetStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Routes.ProcessDetails)] HttpRequest req,
            [DurableClient] IDurableEntityClient client, 
            string processId)
        {
            var entityId = new EntityId(nameof(LongRunningProcessOrchestrator), processId);
            var status = await client.ReadEntityStateAsync<LongRunningProcessOrchestrator>(entityId);

            if(!status.EntityExists || null == status.EntityState)
                return new NotFoundResult();

            var entity = status.EntityState;

            if (entity.Status == Entities.ProcessStatus.Completed)
                return new OkObjectResult(entity);

            return new AcceptedObjectResult(Routes.BuildProcessDetails(processId), entity);
        }

        [FunctionName("RunProcess")]
        public static async Task RunProcess([QueueTrigger(QueueName, Connection = QueueConnectionName)] string message,
            [DurableClient] IDurableEntityClient client)
        {
            var command = Newtonsoft.Json.JsonConvert.DeserializeObject<StartOperation>(message);

            var entityId = new EntityId(nameof(LongRunningProcessOrchestrator), command.RequestId.ToString());

            await client.SignalEntityAsync<ILongRunningProcessOrchestrator>(entityId, e => e.Start(command));
        }
    }
}
