using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using web_api_users.Controllers.Data;

namespace web_api_users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : ControllerBase
    {
        private readonly AppDbContext _db;



        public CategoriaController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("GetAll")]//especificamos para evitar endPoints repetidos
        public async Task<IActionResult> GetCategorias()
        {
            var lista = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();

            if (lista == null)
            {
                return NotFound("NO HAY CATEGORIAS");
            }

            return Ok(lista);
        }
        //[HttpGet("{id:int}")]
        [HttpGet]
        [Route("GetById")]//otro nombre para no indicar los parametros en el TAG
        public async Task<IActionResult> GetCategoriasById(int id)
        {
            var cat = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id);

            if (cat == null)
            {
                return NotFound("NO EXISTE LA CATEGORIA: " + id);
            }

            return Ok(cat);
        }
    }
}
