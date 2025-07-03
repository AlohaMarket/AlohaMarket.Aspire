using Aloha.MicroService.Post.Models.Responses;
using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Requests;
using Aloha.PostService.Models.Responses;
using AutoMapper;

namespace Aloha.PostService.Mapper
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            // Request to Entity mappings
            CreateMap<PostCreateRequest, Models.Entity.Post>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.IsViolation, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PushedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsLocationValid, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.IsCategoryValid, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.IsUserPlanValid, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => PostStatus.PendingValidation))
                .ForMember(dest => dest.ProvinceText, opt => opt.Ignore())
                .ForMember(dest => dest.DistrictText, opt => opt.Ignore())
                .ForMember(dest => dest.WardText, opt => opt.Ignore())
                .ForMember(dest => dest.LocationValidationReceived, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.CategoryValidationReceived, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.UserPlanValidationReceived, opt => opt.MapFrom(_ => false));

            CreateMap<PostUpdateRequest, Models.Entity.Post>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.UserPlanId, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PushedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsLocationValid, opt => opt.Ignore())
                .ForMember(dest => dest.IsCategoryValid, opt => opt.Ignore())
                .ForMember(dest => dest.IsUserPlanValid, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.LocationValidationReceived, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryValidationReceived, opt => opt.Ignore())
                .ForMember(dest => dest.UserPlanValidationReceived, opt => opt.Ignore());

            // Entity to Response mappings
            CreateMap<Models.Entity.Post, PostCreateResponse>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

            CreateMap<Models.Entity.Post, PostDetailResponse>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

            CreateMap<Models.Entity.Post, PostListResponse>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Images.FirstOrDefault(img => img.Order == 1)));

            CreateMap<PostImage, PostImageResponse>();
        }
    }
}
