using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Services.Interfaces
{
    public interface IFileService
    {
        public List<string> GetAllFiles();
        public List<string> GetAllProcessingFiles();
        public void ValidateAndCreateDirectories();
        public void MoveProcessedFile(string file);
        public void MoveCancelledOrder(string file);
        public void MoveInvalidFile(string file);
        public void MoveRegisteredProduct(string file);
        public string MoveToProcessing(string file);
        public void DeleteFile(string file);
        public string GetGuidFromOrderFile(string file);
    }
}
