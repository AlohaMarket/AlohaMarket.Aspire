using Aloha.LocationService.Models.Entities;
using Aloha.LocationService.Models.Responses;
using AutoMapper;

namespace Aloha.LocationService.Mapper
{
    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            CreateMap<Province, ProvinceResponse>()
                .ForMember(dest => dest.Districts, opt => opt.MapFrom(src => src.Districts));

            CreateMap<District, DistrictResponse>()
                .ForMember(dest => dest.Wards, opt => opt.MapFrom(src => src.Wards));

            CreateMap<Ward, WardResponse>();
        }
    }
}