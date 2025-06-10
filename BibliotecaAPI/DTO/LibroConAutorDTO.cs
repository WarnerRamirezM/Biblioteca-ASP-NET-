using System.Reflection.Metadata.Ecma335;

namespace BibliotecaAPI.DTO
{
    public class LibroConAutoresDTO: LibroDTO //herencia la id y titulo del libro
    {
        public List<AutorDTO> Autores { get; set; } = [];
    }
}
