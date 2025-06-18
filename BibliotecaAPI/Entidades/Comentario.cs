using Microsoft.AspNetCore.Identity;
using System.Reflection.Metadata.Ecma335;

namespace BibliotecaAPI.Entidades
{
    public class Comentario
    {
        public Guid Id { get; set; } //string aleatorio
        public required string Cuerpo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int LibroId { get; set; }
        public Libro? Libro { get; set; } //configuracion 1 a muchos DEPENDIENTE donde el comentario DEPENDE de un LIBRO
        //crear relacion entre comentario y usuario
        public required string UsuarioId { get; set; } //obligatorio
        //propiedad de navegacion 
        public Usuario? Usuario { get; set; }
    }
}
