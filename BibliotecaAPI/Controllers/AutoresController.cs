using AutoMapper;
using Azure;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTO;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.JsonPatch;
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
        public async Task<ActionResult<AutorConLibrosDTO>> Get(int id)
        {
            var autor = await context.Autores
                .Include(x => x.Libros)
                .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }
            var autorDTO = mapper.Map<AutorConLibrosDTO>(autor);
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
            return NoContent(); //204 standart API 
        }
        [HttpPatch("{id:int}")] // Indica que este método responde a solicitudes HTTP PATCH con un parámetro 'id' entero en la URL (por ejemplo, PATCH /autores/3)
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AutorPatchDTO> patchDoc) // Método asincrónico que recibe el ID del autor y un documento PATCH con los cambios
        {
            if (patchDoc == null) // Si el documento patch es null, es decir, no se recibió nada del cliente...
            {
                return BadRequest(); // ...se retorna un error 400 (Bad Request), ya que no se puede procesar la solicitud.
            }

            var autorDB = await context.Autores.FirstOrDefaultAsync(x => x.Id == id); // Se busca en la base de datos el autor con el ID especificado
            if (autorDB == null)
            { // Si no se encuentra ningún autor con ese ID...
                return NotFound(); // ...se retorna un error 404 (Not Found)
            }

            var autorPatchDTO = mapper.Map<AutorPatchDTO>(autorDB); // Se mapea el autor de la base de datos a un DTO que puede recibir el patch

            patchDoc.ApplyTo(autorPatchDTO, ModelState); // Se aplican los cambios del documento patch al DTO, y se registra cualquier error en ModelState

            var esValido = TryValidateModel(autorPatchDTO); // Se valida que el modelo con los cambios cumpla con las reglas de validación

            if (!esValido) // Si el modelo no es válido después de aplicar el patch...
            {
                return ValidationProblem(); // ...se retorna un error 400 con los detalles de validación
            }

            mapper.Map(autorPatchDTO, autorDB); // Se mapea nuevamente el DTO al modelo original para actualizar sus valores con los cambios válidos

            await context.SaveChangesAsync(); // Se guardan los cambios en la base de datos de forma asincrónica

            return NoContent(); // Se retorna 204 (No Content), indicando que la operación fue exitosa pero no hay contenido que devolver
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                return NotFound(); 
            }
             
            return NoContent(); //no retorna nada porque ya no existe 
        }
    }
}
