using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTO;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        public LibrosController(ApplicationDbContext context, IMapper mapper) //inyeccion de automaper
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<LibroDTO>> Get()
        {
            var libros = await context.Libros.ToListAsync();
            var librosDTO = mapper.Map<IEnumerable<LibroDTO>>(libros);
            return librosDTO;
        }

        [HttpGet("{id:int}", Name = "ObtenerLibro")]
        public async Task<ActionResult<LibroConAutoresDTO>> Get(int id)
        {
            var libro = await context.Libros
                .Include(x => x.Autores)
                .ThenInclude(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
            {
                return NotFound();
            }
            var libroDTO = mapper.Map<LibroConAutoresDTO>(libro);
            return libroDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds is null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), "No se puede crear un libro sin autores");
                return ValidationProblem();
            }
            var autoresIdsExisten = await context.Autores.Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            if (autoresIdsExisten.Count != libroCreacionDTO.AutoresIds.Count)
            {
                var autoresNoExisten = libroCreacionDTO.AutoresIds.Except(autoresIdsExisten);
                var autoresNoExistenString = string.Join(", ", autoresNoExisten);
                var mensajeError = $"Los siguientes autores no existen:{autoresNoExistenString}";
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensajeError);
                return ValidationProblem();

            }
            var libro = mapper.Map<Libro>(libroCreacionDTO); //convierte el objeto por parametro a un libro para guardar en bbdd

            AsignarOrdenAutores(libro);
            context.Add(libro); //agrega el libro
            await context.SaveChangesAsync(); //guarda el libro en la bbdd
            var libroDTO = mapper.Map<LibroDTO>(libro); //Convierte la entidad Libro recién guardada en un LibroDTO, que se usará para enviar la respuesta al cliente sin exponer la entidad completa
            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO); //devuelve el objeto libroDTO como contenido de la respuesta
        }
        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.Autores is not null)
            {
                for (int i = 0; i < libro.Autores.Count; i++)
                {
                    libro.Autores[i].Orden = i;
                }
            }
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds is null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), "No se puede crear un libro sin autores");
                return ValidationProblem();
            }
            var autoresIdsExisten = await context.Autores.Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            if (autoresIdsExisten.Count != libroCreacionDTO.AutoresIds.Count)
            {
                var autoresNoExisten = libroCreacionDTO.AutoresIds.Except(autoresIdsExisten);
                var autoresNoExistenString = string.Join(", ", autoresNoExisten);
                var mensajeError = $"Los siguientes autores no existen:{autoresNoExistenString}";
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensajeError);
                return ValidationProblem();

            }

            //traer la data de la base de datos
            var libroDB = await context.Libros.Include(x => x.Autores)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
            {
                return NotFound();
            }
            libroDB = mapper.Map(libroCreacionDTO, libroDB);
            AsignarOrdenAutores(libroDB);
           
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
            public async Task<ActionResult> Delete(int id)
            {
                var registrosBorrados = await context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();

                if (registrosBorrados == 0)
                {
                    return NotFound();
                }

                return NoContent(); //no devuelve ningun contenido
            }
        }
    }
