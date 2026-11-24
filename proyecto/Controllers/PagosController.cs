
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using proyecto.Dto;
using proyecto.Services;

namespace proyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoReservacionService pagoService;

        public PagosController(IPagoReservacionService pagoService)
        {
            this.pagoService = pagoService;
        }

        // POST: api/Pagos/Aplicar
        [HttpPost("Aplicar")]
        public async Task<IActionResult> AplicarPago(
            [FromBody] PagoRequestDTO request,
            CancellationToken cancellationToken)
        {
            try
            {
                string? usuarioClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

                if (!int.TryParse(usuarioClaim, out int usuarioId))
                    return Unauthorized(new
                    {
                        mensaje = "No se pudo identificar al usuario administrador."
                    });

                var resultado = await pagoService.AplicarPagoAsync(
                    request,
                    usuarioId,
                    cancellationToken
                );

                return Ok(new
                {
                    mensaje = "Pago registrado correctamente.",
                    pago = resultado
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error al registrar el pago."
                });
            }
        }

        // GET: api/Pagos/Historial/1
        [HttpGet("Historial/{reservacionId}")]
        public async Task<IActionResult> ObtenerHistorial(
            int reservacionId,
            CancellationToken cancellationToken)
        {
            try
            {
                var historial = await pagoService.ObtenerHistorialAsync(
                    reservacionId,
                    cancellationToken
                );

                return Ok(historial);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error al consultar el historial."
                });
            }
        }
    }
}
