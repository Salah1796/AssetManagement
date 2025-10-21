using AutoMapper;
using CommandsService.Dtos;
using CommandsService.Models;
namespace CommandsService.Profiles
{
    public class CommandsProfile : Profile
    {
        public CommandsProfile()
        {
            // Source -> Target
            CreateMap<Platform, PlatformreadDto>();
            CreateMap<CommandCreateDto, Command>();
            CreateMap<Command, CommandReadDto>();

            CreateMap<PlatformPublishedDto, Platform>()
                .ForMember(dto => dto.ExternalID, src => src.MapFrom(x => x.Id));

            CreateMap<GrpcPlatformModel, Platform>()
               .ForMember(dest => dest.ExternalID, opt => opt.MapFrom(src => src.PlatformId))
               .ForMember(dest => dest.Commands, opt => opt.Ignore());
        }
    }
}