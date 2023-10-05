using AutoMapper;
using ByteSyncer.Core.Files.Commands;
using ByteSyncer.Domain.Files.Models;

namespace ByteSyncer.Application.Files.Mappings
{
    public class FolderProfile : Profile
    {
        public FolderProfile()
        {
            CreateMap<Folder, Folder>();
            CreateMap<Folder, CreateFolderCommand>().ReverseMap();
        }
    }
}
