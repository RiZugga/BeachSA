using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto.Dto;
using proyecto.Models;

namespace proyecto.Controllers
{
    [ApiController]
    [Route("api/paquetes")]
    public class PaquetesController : ControllerBase
    {
        private readonly DbContextBank context;

        public PaquetesController(DbContextBank context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Paquete>>> ObtenerPaquetesAsync()
        {
            return await context.Paquetes.ToListAsync();
        }

        [HttpGet("nombre/{nombre}")]
        public async Task<ActionResult<Paquete>> ObtenerPaquetePorIdAsync(string nombre)
        {
            var paquete = await context.Paquetes.FirstOrDefaultAsync(p => p.Nombre == nombre);
            if (paquete == null)
                return NotFound();

            return Ok(paquete);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Paquete>> ObtenerPaquetePorIdAsync(int id)
        {
            var paquete = await context.Paquetes.FindAsync(id);
            if (paquete == null)
                return NotFound();

            return Ok(paquete);
        }

        [HttpPost]
        public async Task<ActionResult<Paquete>> CrearPaqueteAsync(PaqueteDTO request)
        {
            if (await context.Paquetes.AnyAsync(p => p.Nombre == request.Nombre))
                return Conflict("Ya existe un paquete con ese nombre.");

            var paquete = new Paquete
            {
                Nombre = request.Nombre,
                PrecioPersonaNoche = request.PrecioPersonaNoche,
                PorcentajePrima = request.PorcentajePrima,
                CantidadMensualidades = request.CantidadMensualidades,
                Activo = true
            };

            context.Paquetes.Add(paquete);
            await context.SaveChangesAsync();
            return Ok("Paquete creado correctamente.");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<string>> ActualizarPaqueteAsync(int id, PaqueteDTO request)
        {
            var paquete = await context.Paquetes.FindAsync(id);
            if (paquete == null)
                return NotFound();

            if (await context.Paquetes.AnyAsync(p => p.Nombre == request.Nombre && p.Id != id))
                return Conflict("Ya existe un paquete con ese nombre.");

            paquete.Nombre = request.Nombre;
            paquete.PrecioPersonaNoche = request.PrecioPersonaNoche;
            paquete.PorcentajePrima = request.PorcentajePrima;
            paquete.CantidadMensualidades = request.CantidadMensualidades;
            await context.SaveChangesAsync();

            return Ok("Paquete actualizado correctamente.");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarPaqueteAsync(int id)
        {
            var paquete = await context.Paquetes.FindAsync(id);
            if (paquete == null)
                return NotFound();

            paquete.Activo = false;
            await context.SaveChangesAsync();
            return Ok("Paquete desactivado correctamente.");
        }
    }
}
