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
                config => config.MapFrom(autor => MapearNombreYApellidoAutor(autor)));

            CreateMap<Autor, AutorConLibrosDTO>()
                .ForMember(dto => dto.NombreCompleto,
                config => config.MapFrom(autor => MapearNombreYApellidoAutor(autor)));

            CreateMap<AutorCreacionDTO, Autor>(); //desde el DTO a la entidad AUTOR
            CreateMap<Autor, AutorPatchDTO>().ReverseMap();
            CreateMap<AutorLibro, LibroDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.LibroId)) //autorLibro.libroid
                .ForMember(dto => dto.Titulo, config => config.MapFrom(ent => ent.Libro!.Titulo));
            CreateMap<LibroCreacionDTO, AutorLibro>()
                .ForMember(ent => ent.Libro, 
                    config => config.MapFrom(dto => new Libro { Titulo = dto.Titulo }));



            CreateMap<Libro, LibroDTO>();

            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(ent => ent.Autores, config =>
                config.MapFrom(dto => dto.AutoresIds.Select(id => new AutorLibro { AutorId = id })));
            CreateMap<Libro, LibroConAutoresDTO>();

            CreateMap<AutorLibro, AutorDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AutorId))
                .ForMember(dto => dto.NombreCompleto,
                config => config.MapFrom(ent => MapearNombreYApellidoAutor(ent.Autor!)));

            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>()
                .ForMember(dto => dto.UsuarioEmail, config => config.MapFrom(ent => ent.Usuario!.Email));
            CreateMap<ComentarioPatchDTO, Comentario>().ReverseMap();
            //de usuario a usuarioDTO
            CreateMap<Usuario, UsuarioDTO>();

        }
        private string MapearNombreYApellidoAutor(Autor autor) => $"{autor.Nombres} {autor.Apellidos}";
    }
}
