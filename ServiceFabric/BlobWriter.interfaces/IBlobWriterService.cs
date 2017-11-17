
using CommonResources;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading;
using System.Threading.Tasks;

namespace BlobWriter.interfaces
{
    public interface IBlobWriterService : IService
    {
        
        Task ReceiveMessageAsync(DeviceMessage message, CancellationToken cancellationToken);
        
    }
}
