using AutoMapper;
using BibliotecaAPI.DTO;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;

namespace BibliotecaAPI.Utilidades
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Autor, AutorDTO>()
                .ForMember(dto => dto.NombreCompleto,
                config => config.MapFrom(autor => $"{autor.Nombres} {autor.Apellidos}"));

            CreateMap<AutorCreacionDTO, Autor>(); //desde el DTO a la entidad AUTOR
        }

    }
}
