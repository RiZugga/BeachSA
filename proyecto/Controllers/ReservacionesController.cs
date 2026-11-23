
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

//model layer DTO
using proyecto.Dto;

//services layer
using proyecto.Services;

namespace proyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        //variable para el servicio de reservaciones
        private readonly IReservacionService reservacionService;

        //constructor por omision del controller
        public ReservasController(IReservacionService pReservacionService)
        {
            //se instancia el servicio mediante inyeccion de dependencias
            this.reservacionService = pReservacionService;
        }

        //Method List
        [HttpGet("List")]
        public async Task<IActionResult> List()
        {
            try
            {
                //variable tipo lista
                List<ReservacionResponseDTO> reservacionList =
                    await this.reservacionService.ListarAsync();

                //se devuelve la lista en formato JSON
                return Ok(reservacionList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //Method SearchID
        [HttpGet("SearchID")]
        public async Task<IActionResult> SearchID(int id)
        {
            try
            {
                //validacion del ID
                if (id <= 0)
                {
                    return BadRequest("El ID de la reservacion no es valido");
                }

                //se busca la reservacion mediante el servicio
                ReservacionResponseDTO temp =
                    await this.reservacionService.BuscarPorIdAsync(id);

                //se devuelve el objeto en formato JSON
                return Ok(temp);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //Method Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(
            [FromBody] ReservacionDTO temp,
            CancellationToken cancellationToken)
        {
            try
            {
                //validacion de los datos
                if (temp == null)
                {
                    return BadRequest("No se permiten datos en blanco");
                }

                //se obtiene el ID del usuario autenticado
                string usuarioClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                //validacion del usuario
                if (!int.TryParse(usuarioClaim, out int usuarioId))
                {
                    return Unauthorized("No se pudo identificar al usuario");
                }

                //se utiliza el servicio para crear la reservacion
                ReservacionResponseDTO resultado =
                    await this.reservacionService.CrearAsync(
                        temp, usuarioId, cancellationToken);

                //se devuelve la reservacion creada
                return CreatedAtAction(
                    nameof(SearchID),
                    new { id = resultado.Id },
                    resultado
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (TipoCambioNoDisponibleException ex)
            {
                return StatusCode(503, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //Method Update
        [HttpPut("Update")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] ReservacionDTO temp,
            CancellationToken cancellationToken)
        {
            try
            {
                //validaciones
                if (id <= 0 || temp == null)
                {
                    return BadRequest("Los datos de la reservacion no son validos");
                }

                //se utiliza el servicio para modificar los datos
                ReservacionResponseDTO resultado =
                    await this.reservacionService.EditarAsync(
                        id, temp, cancellationToken);

                //se devuelve la reservacion modificada
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (TipoCambioNoDisponibleException ex)
            {
                return StatusCode(503, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //Method Delete
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                //validacion del ID
                if (id <= 0)
                {
                    return BadRequest("El ID no es valido");
                }

                //se utiliza el servicio para eliminar la reservacion
                await this.reservacionService.EliminarAsync(id);

                //se confirma la eliminacion
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //Method Cotizar
        [HttpPost("Cotizar")]
        public async Task<IActionResult> Cotizar(
            [FromBody] CotizacionRequestDTO temp,
            CancellationToken cancellationToken)
        {
            try
            {
                //validacion de los datos
                if (temp == null)
                {
                    return BadRequest("Los datos de cotizacion son obligatorios");
                }

                //se utiliza el servicio para calcular la cotizacion
                CotizacionDTO resultado =
                    await this.reservacionService.CotizarAsync(
                        temp, cancellationToken);

                //se devuelve el resultado
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (TipoCambioNoDisponibleException ex)
            {
                return StatusCode(503, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }//close controller
}//close namespace
