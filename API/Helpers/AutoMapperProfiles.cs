using API.Dtos;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace API;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(dest => dest.PhotoUrl,
                       opt => opt.MapFrom(src => src.Photos
                       .FirstOrDefault(x => x.IsMain).Url))
            .ForMember(dest => dest.Age,
                       opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();

        CreateMap<CommentList, DriveCommentListDTO>()
            .ForMember(dest => dest.Comments, opt =>
            {
                opt.MapFrom(src => src.Comments);
            });
        CreateMap<DriveCommentDTO, Comment>();
        CreateMap<DriveAddCommentDTO, Comment>();
        CreateMap<Comment, DriveCommentDTO>();
 
        CreateMap<Reply, DriveReplyDTO>();
        CreateMap<User, DriveUserDTO>();
        CreateMap<RevisionList, DriveRevisionListDTO>()
            .ForMember(dest => dest.Revisions, opt =>
            {
                opt.MapFrom(src => src.Revisions);
            });
        CreateMap<DriveRevisionDto, Revision>();
        CreateMap<Revision, DriveRevisionDto>()
            .ForMember(dest => dest.UploadedBy, opt => opt.MapFrom<CustomUploadedByResolver>());
        CreateMap<Change, DriveChangesDto>();
        CreateMap<DriveChangesDto, Change>();
        CreateMap<LastModifyingUser, User>();
        CreateMap<User, LastModifyingUser>();
        CreateMap<Google.Apis.Drive.v3.Data.File, FileChangesDTO>();
    }
    public class CustomUploadedByResolver : IValueResolver<Revision, DriveRevisionDto, string>
    { 
        public string Resolve(Revision source, DriveRevisionDto destination, string destMember, ResolutionContext context)
        {
            return context.Items["modifiedBy"] as string;
        }
    }
}
