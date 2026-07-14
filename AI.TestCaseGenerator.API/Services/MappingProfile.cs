using AI.TestCaseGenerator.API.DTOs.AIChat;
using AI.TestCaseGenerator.API.DTOs.Document;
using AI.TestCaseGenerator.API.DTOs.Project;
using AI.TestCaseGenerator.API.DTOs.TestCase;
using AI.TestCaseGenerator.API.DTOs.User;
using AI.TestCaseGenerator.API.Entities;
using AutoMapper;

namespace AI.TestCaseGenerator.API.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserProfileDto>();

            CreateMap<Project, ProjectResponseDto>()
                .ForMember(dest => dest.TotalDocuments, opt => opt.MapFrom(src => src.Documents.Count))
                .ForMember(dest => dest.TotalTestCases, opt => opt.MapFrom(src => src.TestCases.Count))
                .ForMember(dest => dest.TotalChats, opt => opt.MapFrom(src => src.ChatHistories.Count));

            CreateMap<Document, DocumentResponseDto>()
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<TestCase, TestCaseResponseDto>();

            CreateMap<ChatHistory, ChatHistoryDto>()
                .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.UserQuestion))
                .ForMember(dest => dest.Answer, opt => opt.MapFrom(src => src.AiResponse));
        }
    }
}
