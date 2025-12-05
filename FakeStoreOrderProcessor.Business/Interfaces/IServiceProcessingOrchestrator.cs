using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Interfaces
{
    public interface IServiceProcessingOrchestrator
    {
        public Task EventHandlerAsync();
        public void SignalStop();
    }
}
