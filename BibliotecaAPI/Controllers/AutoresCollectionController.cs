using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTO;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/autores-coleccion")]
    //la politica obliga al claim jtw : "esadmin" -> program. builder.Services.AddAuthorization
    [Authorize(Policy = "esadmin")] //proteger todas las acciones con ciertas credenciales (login,administrador..)
    public class AutoresCollectionController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AutoresCollectionController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        [HttpGet("{ids}", Name = "ObtenerAutoresPorIds")] //api/autores-coleccion/1,2,3
        public async Task<ActionResult<List<AutorConLibrosDTO>>> Get(string ids)
        {
            var idsColeccion = new List<int>();
            foreach (var id in ids.Split(",")) //separarlos por comas
            {
                if(int.TryParse(id, out int idInt))
                {
                    idsColeccion.Add(idInt);
                }
            }
            if (idsColeccion.Any()) //si la lista de ids no tiene ninguno
            {
                ModelState.AddModelError(nameof(ids), "Ningun id fue encontrado");
            }
            var autores = await context.Autores
                .Include(x=> x.Libros)
                .ThenInclude(x=> x.Libro)
                .Where(x => idsColeccion.Contains(x.Id))
                .ToListAsync();

            if(autores.Count != idsColeccion.Count) //si la cantidad es distinta es que no se encontraron autores
            {
                return NotFound();
            }
            var autorDTO = mapper.Map<List<AutorConLibrosDTO>> (autores);
            return autorDTO;



        }

        [HttpPost]
        public async Task<ActionResult> Post(IEnumerable<AutorCreacionDTO> autoresCreacionDTO)
        {
            var autores = mapper.Map<IEnumerable<Autor>> (autoresCreacionDTO);
            context.AddRange(autores); //agrega un listado de entidades
            await context.SaveChangesAsync();  
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>> (autores);
            var ids = autores.Select(x => x.Id);
            var idsString = string.Join(",", ids);

            return CreatedAtRoute("ObtenerAutoresPorIds", new { ids = idsString }, autoresDTO);

        }


    }
}
