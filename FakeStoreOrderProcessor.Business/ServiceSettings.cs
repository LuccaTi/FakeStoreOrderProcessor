using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business
{
    /// <summary>
    /// Map parameters from ServiceSettings section in appsettings.json.
    /// This class can be injected using IOptions<ServiceSettings>.
    /// </summary>
    public class ServiceSettings
    {
        public int Interval { get; set; }
    }
}
