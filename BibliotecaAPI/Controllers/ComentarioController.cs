using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTO;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")] //para acceder a los comentarios 
    [Authorize] //proteccion de acciones
    public class ComentarioController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public ComentarioController(ApplicationDbContext context, IMapper mapper, IServiciosUsuarios serviciosUsuarios) //control . create and asing fields
        {
            this.context = context;
            this.mapper = mapper;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro) //buscamos el libro
            {
                return NotFound(); //si no existe, retorna no encontrado
            }
            //si el libro existe continuamos y traemos la data relacionada con el usuario 
            var comentarios = await context.Comentarios
                .Include(x => x.Usuario) //datos del usuario incluidos
                .Where(x => x.LibroId == libroId)
                .OrderByDescending(x => x.FechaPublicacion)
                .ToListAsync();
            return mapper.Map<List<ComentarioDTO>>(comentarios); //El recurso es comentarios DDBB y lo mapea a comentarioDTO
        }
        [HttpGet("{id}", Name = "ObtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> Get(Guid id)
        {
            var comentario = await context.Comentarios
                                    .Include (x => x.Usuario) //incluimos la data del usuario
                                    .FirstOrDefaultAsync(x => x.Id == id); //devolvemos segun el id encontrado en ComentarioDDBB
            if(comentario is null) //si el comentario no existe devolvemos no encontrado
            {
                return NotFound();
            }
            //si existe mapeamos a un comentarioDTO para enviar los datos al cliente
            return mapper.Map<ComentarioDTO>(comentario);
        }
        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var libroExiste = await context.Libros.AnyAsync(x => x.Id == libroId); //buscamos el libro en la base de datos
            //validamos si existe el libro
            if (!libroExiste)
            {
                return NotFound();
            }
            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return NotFound();
            }

            //si el libro existe insertamos el comentario
            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO); //mapeamos un comentarioCreacionDTO a un ComentarioDDBB de la base de datos
            comentario.LibroId = libroId; //asignamos el Id del libro al comentario
            comentario.FechaPublicacion = DateTime.UtcNow;
            comentario.UsuarioId = usuario.Id; //asignamos el id del usuario 
            context.Add(comentario); //insertamos el comentario 
            context.SaveChanges(); //actualizamos la base de datos con el comentario guardado

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario); //mapeamos el comentarioDDBB a un comentario DTO para mostarlo al cliente
            return CreatedAtRoute("ObtenerComentario", new { id = comentario.Id, libroId }, comentarioDTO); //Marca la ruta del Get y asigna el identificador para crear la ruta correcta

        }
        [HttpPatch("{id}")] // Indica que este método responde a solicitudes HTTP PATCH con un parámetro 'id' entero en la URL (por ejemplo, PATCH /autores/3)
        public async Task<ActionResult> Patch(Guid id,int libroId, JsonPatchDocument<ComentarioPatchDTO> patchDoc) // 
        {
            if (patchDoc == null) // Si el documento patch es null, es decir, no se recibió nada del cliente...
            {
                return BadRequest(); // ...se retorna un error 400 (Bad Request), ya que no se puede procesar la solicitud.
            }
            var libroExiste = await context.Libros.AnyAsync(x => x.Id == libroId); //buscamos el libro en la base de datos
            //validamos si existe el libro
            if (!libroExiste)
            {
                return NotFound();
            }
            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return NotFound();
            }
            //el usuario que escribio el comentario es el unico que puede editar su mismo comentario


            var comentarioDB = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id); // Se busca en la base de datos el autor con el ID especificado
            if (comentarioDB == null)
            { // Si no se encuentra ningún autor con ese ID...
                return NotFound(); // ...se retorna un error 404 (Not Found)
            }
            if(comentarioDB.UsuarioId != usuario.Id) //si el usuario del comentario es distinto al usuario 
            { 
                return Forbid(); //retorna un forbid o prohibido.
            }
            var comentarioPatchDTO = mapper.Map<ComentarioPatchDTO>(comentarioDB); // Se mapea el comentario de la base de datos a un DTO que puede recibir el patch

            patchDoc.ApplyTo(comentarioPatchDTO, ModelState); // Se aplican los cambios del documento patch al DTO, y se registra cualquier error en ModelState

            var esValido = TryValidateModel(comentarioPatchDTO); // Se valida que el modelo con los cambios cumpla con las reglas de validación

            if (!esValido) // Si el modelo no es válido después de aplicar el patch...
            {
                return ValidationProblem(); // ...se retorna un error 400 con los detalles de validación
            }

            mapper.Map(comentarioPatchDTO, comentarioDB); // Se mapea nuevamente el DTO al modelo original para actualizar sus valores con los cambios válidos

            await context.SaveChangesAsync(); // Se guardan los cambios en la base de datos de forma asincrónica

            return NoContent(); // Se retorna 204 (No Content), indicando que la operación fue exitosa pero no hay contenido que devolver
        }
        [HttpDelete]
        public async Task<ActionResult> Delete(Guid id, int libroId)
        {
            var libroExiste = await context.Libros.AnyAsync(x => x.Id == libroId); //buscamos el libro en la base de datos
            //validamos si existe el libro
            if (!libroExiste)
            {
                return NotFound();
            }
            //verificacion del usuario 
            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return NotFound();
            }
            //un usuario solo puede borrar su propio comentario
            var comentarioDB = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentarioDB is null)
            {
                return NotFound();
            }
            if(comentarioDB.Usuario.Id != usuario.Id)
            {
                return Forbid();
            }
            context.Remove(comentarioDB); //se marca la entidad para ser borrado cuando se haga un savechange
            await context.SaveChangesAsync(); //borrado

            return NoContent(); //De lo contrario un nonContent

        }

    }
}
