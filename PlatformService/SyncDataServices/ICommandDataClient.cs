using PlatformService.Dtos;

namespace PlatformService.SyncDataServices
{
    public interface ICommandDataClient
    {
        Task SendPlatformToCommand(PlatformReadDto plat); 
    }
}