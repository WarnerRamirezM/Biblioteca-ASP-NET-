using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTO;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/autores")]
    public class AutoresController: ControllerBase
    {
        private readonly ApplicationDbContext context; //campo de clase para poder acceder en toda la clase
        private readonly IMapper mapper; //imyeccion de AutoMapper
        public AutoresController(ApplicationDbContext context, IMapper mapper) //inyectando el applicationDBContext
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            var autoresDTO = autores.Select(autor => 
            new AutorDTO { Id = autor.Id, NombreCompleto = $"{autor.Nombres} {autor.Apellidos}"});

            return autoresDTO;

        }
        /*
        [HttpGet]
        public async Task<IEnumerable<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);

            return autoresDTO;

        }*/

        [HttpGet("{id:int}", Name = "ObtenerAutor")] // api/autores/id
        public async Task<ActionResult<AutorDTO>> Get(int id)
        {
            var autor = await context.Autores
                .Include(x => x.Libros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return autorDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor> (autorCreacionDTO);
            context.Add(autor); //agregamos el autor
            await context.SaveChangesAsync(); //guardamos cambios asyn a nivel de bases de datos
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new {id = autor.Id}, autor); //creado por ruta, id cliente, autor creado
        }

        [HttpPut("{id:int}")] // api/autores/id
        public async Task<ActionResult> Put(int id, AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;
            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                return NotFound(); 
            }
             
            return Ok();
        }
    }
}
