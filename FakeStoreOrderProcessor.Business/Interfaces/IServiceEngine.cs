using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Interfaces
{
    public interface IServiceEngine
    {
        public Task ProcessAsync(CancellationToken cancellationToken);
    }
}
