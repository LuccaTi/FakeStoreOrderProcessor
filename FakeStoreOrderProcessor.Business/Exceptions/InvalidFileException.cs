using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Exceptions
{
    internal class InvalidFileException : Exception
    {
        public InvalidFileException(string message) : base(message)
        {

        }
    }
}
