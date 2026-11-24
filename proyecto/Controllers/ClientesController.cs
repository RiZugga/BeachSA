using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto.Dto;
using proyecto.Models;
using System.Text.Json;

namespace proyecto.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly DbContextBank context;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<ClientesController> logger;

        public ClientesController(DbContextBank context, IHttpClientFactory httpClientFactory, ILogger<ClientesController> logger)
        {
            this.context = context;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Cliente>>> ObtenerClientesAsync()
        {
            return await context.Clientes.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Cliente>> ObtenerClientePorIdAsync(int id)
        {
            var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null){
                    return NotFound();
                }
            
            return Ok(cliente);
        }

        [HttpGet("existscedula")]
        public async Task<ActionResult<bool>> ExisteCedulaAsync([FromQuery] string value)
        {
            var existe = await context.Clientes.AnyAsync(c => c.Cedula == value);
            return Ok(existe);
        }

        [HttpGet("searchdescrip/{value}")]
        public async Task<ActionResult<List<Cliente>>> BuscarClientesPorNombreAsync(string value)
        {
            return await context.Clientes
                .Where(c => c.NombreCompleto.Contains(value))
                .ToListAsync();
        }

        [HttpGet("consulta-cedula/{cedula}")]
        public async Task<IActionResult> ConsultarCedulaAsync(string cedula)
        {
            var resultado = await ObtenerNombrePorCedulaAsync(cedula);
            if (resultado.Result != null)
                return resultado.Result;

            return Ok(new { nombre = resultado.Value });
        }

        private async Task<ActionResult<string>> ObtenerNombrePorCedulaAsync(string cedula)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                using var response = await client.GetAsync(
                    $"https://apis.gometa.org/cedulas/{Uri.EscapeDataString(cedula)}");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(StatusCodes.Status502BadGateway, "No se pudo consultar la cédula en GoMeta.");
                }

                //Stream de la respuesta JSON de GoMeta
                await using var stream = await response.Content.ReadAsStreamAsync();

                // Parse del JSON de GoMeta
                using var document = await JsonDocument.ParseAsync(stream);


                if (!document.RootElement.TryGetProperty("nombre", out var nombre))
                    return NotFound("GoMeta no encontró un nombre para la cédula.");


                var nombreCompleto = nombre.GetString();
                
                if (string.IsNullOrWhiteSpace(nombreCompleto))
                    return NotFound("GoMeta no encontró un nombre para la cédula.");

                return nombreCompleto;
            }

            //Error de conexión con GoMeta
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Error al conectar con GoMeta");
                return StatusCode(StatusCodes.Status502BadGateway, "No se pudo conectar con GoMeta.");
            }

            //Error de JSON
            catch (JsonException ex)
            {
                logger.LogError(ex, "GoMeta devolvió una respuesta JSON inválida");
                return StatusCode(StatusCodes.Status502BadGateway, "GoMeta devolvió una respuesta inválida.");
            }

            //Timeout
            catch (TaskCanceledException)
            {
                return StatusCode(StatusCodes.Status504GatewayTimeout, "La consulta a GoMeta agotó el tiempo de espera.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> CrearClienteAsync(CrearClienteDTO request)
        {
            if (await context.Clientes.AnyAsync(c => c.Cedula == request.Cedula))
                return Conflict("Ya existe un cliente con esa cedula.");

            var resultado = await ObtenerNombrePorCedulaAsync(request.Cedula);
            if (resultado.Result != null)
                return resultado.Result;

            var nombreCompleto = resultado.Value;
            if (string.IsNullOrWhiteSpace(nombreCompleto) || nombreCompleto.Length > 150)
            {
                logger.LogWarning("GoMeta devolvió un nombre inválido para crear el cliente");
                return StatusCode(StatusCodes.Status502BadGateway, "GoMeta devolvió un nombre inválido.");
            }

            var cliente = new Cliente
            {
                Cedula = request.Cedula,
                TipoCedula = request.TipoCedula,
                NombreCompleto = nombreCompleto,
                Telefono = request.Telefono,
                Direccion = request.Direccion,
                Email = request.Email,
                Activo = true
            };

            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
            return Ok("Cliente creado correctamente.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<string>> ActualizarClienteAsync(int id, ClientDTO request)
        {
            var cliente = await context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            if (await context.Clientes.AnyAsync(c => c.Cedula == request.Cedula && c.Id != id))
                return Conflict("Ya existe un cliente con esa cedula.");

            cliente.Cedula = request.Cedula;
            cliente.TipoCedula = request.TipoCedula;
            cliente.NombreCompleto = request.NombreCompleto;
            cliente.Telefono = request.Telefono;
            cliente.Direccion = request.Direccion;
            cliente.Email = request.Email;

            await context.SaveChangesAsync();

            return Ok("Cliente actualizado correctamente.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarClienteAsync(int id)
        {
            var cliente = await context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            cliente.Activo = false;
            await context.SaveChangesAsync();
            return Ok("Cliente eliminado correctamente.");
        }

        
    }
}
