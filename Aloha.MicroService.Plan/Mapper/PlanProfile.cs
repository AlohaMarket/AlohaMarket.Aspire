using Aloha.MicroService.Plan.Models.Entities;
using Aloha.MicroService.Plan.Models.Request;
using Aloha.MicroService.Plan.Models.Response;
using AutoMapper;

namespace Aloha.MicroService.Plan.Mapper
{
    public class PlanProfile : Profile
    {
        public PlanProfile()
        {
            CreateMap<CreatePlanRequest, Plans>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                 .ForMember(dest => dest.UserPlans, opt => opt.Ignore())
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                 .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdatePlanRequest, Plans>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserPlans, opt => opt.Ignore())
                .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
                .ForMember(dest => dest.DurationDays, opt => opt.Ignore());

            CreateMap<Plans, PlanResponse>();
            CreateMap<UserPlan, UserPlanResponse>()
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan.Name))
                .ForMember(dest => dest.RemainingPosts,
                    opt => opt.MapFrom(src => src.Plan.MaxPosts - src.UsedPosts));
        }
    }


}
