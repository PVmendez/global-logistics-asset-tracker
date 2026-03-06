using AutoMapper;
using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Domain.Entities;

namespace Lexart.GLAT.Api.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Asset, AssetDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateAssetRequest, Asset>();
        CreateMap<UpdateAssetRequest, Asset>();

        CreateMap<TelemetryLog, TelemetryLogDto>()
            .ForMember(d => d.SensorStatus, opt => opt.MapFrom(s => s.SensorStatus))
            .ForMember(d => d.Alerts, opt => opt.MapFrom(s => s.Alerts));

        CreateMap<SensorStatus, SensorStatusDto>();
        CreateMap<CreateSensorStatusRequest, SensorStatus>();
        
        CreateMap<GpsCoordinates, GpsCoordinatesDto>().ReverseMap();
        CreateMap<Alert, AlertDto>()
            .ForMember(d => d.Severity, opt => opt.MapFrom(s => s.Severity.ToString()));
    }
}
