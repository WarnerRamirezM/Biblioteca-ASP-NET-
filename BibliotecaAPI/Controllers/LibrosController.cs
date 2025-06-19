using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTO;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/libros")]
    //la politica obliga al claim jtw : "esadmin" -> program. builder.Services.AddAuthorization
    [Authorize(Policy = "esadmin")] //proteger todas las acciones con ciertas credenciales (login,administrador..)
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ITimeLimitedDataProtector protectorLimitadoPorTiempo;

        public LibrosController(ApplicationDbContext context, IMapper mapper,
            IDataProtectionProvider protectionProvider) //inyeccion de automaper
        {
            this.context = context;
            this.mapper = mapper;
            protectorLimitadoPorTiempo = protectionProvider.CreateProtector("LibrosController").ToTimeLimitedDataProtector();
        }
        [HttpGet("listado/obtenerToken")]
        public ActionResult ObtenerTokenListado()
        {
            var textoPlano = Guid.NewGuid().ToString(); //Cremos un texto guid
            var token = protectorLimitadoPorTiempo.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(30)); //ciframos el token limitado por tiempo
            var url = Url.RouteUrl("ObtenerListadoLibrosUsandoToken", new { token }, "https"); //creamos la ruta completa y pasamos el token
            return Ok(new { url });
        }
        //puede ser utilizado por cualquier persona que tenga el token
        [HttpGet("Listado/{token}", Name = "ObtenerListadoLibrosUsandoToken")]
        [AllowAnonymous]
        public async Task<ActionResult> ObtenerListadoUsandoToken(string token)
        {
            //valimos el token de manera string pasado por parametros para no buscarlo en la base de datos 
            //permitiendo ser mas escalable
            try
            {
                //si marca un error el try el token se encuentra desprotegido
                protectorLimitadoPorTiempo.Unprotect(token);
            }
            catch (Exception)
            {
                //manda el error
                ModelState.AddModelError(nameof(token), "El token ha expirado");
                return ValidationProblem();
            }
            var libros = await context.Libros.ToListAsync();
            var libroDTO = mapper.Map<IEnumerable<LibroDTO>>(libros);
            return Ok(libroDTO);
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
