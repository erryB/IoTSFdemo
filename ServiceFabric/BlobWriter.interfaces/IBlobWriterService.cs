
using CommonResources;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace BlobWriter.interfaces
{
    public interface IBlobWriterService : IService
    {
        
        Task<string> ReceiveMessageAsync(DeviceMessage message);
        
    }
}
