using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Entidades
{
    [PrimaryKey(nameof(AutorId), nameof(LibroId))] //no van haber 2 registros que tengan el mismo autor y el mismo libro
    public class AutorLibro
    {
        //relacion muchos a muchos
        
        public int AutorId { get; set; }
        public int LibroId { get; set; }
        public int Orden { get; set; }
        public Autor? Autor { get; set; }
        public Libro? Libro { get; set; }

    }
}
