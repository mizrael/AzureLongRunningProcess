using System;

namespace AzureLongRunningProcess.Commands
{
    public class StartOperation
    {
        public StartOperation(Guid requestId)
        {
            RequestId = requestId;
        }

        public Guid RequestId { get; }
    }
}