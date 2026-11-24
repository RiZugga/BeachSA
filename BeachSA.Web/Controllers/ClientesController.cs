using Microsoft.AspNetCore.Mvc;
using BeachSA.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using BeachSA.Web.Services;

namespace BeachSA.Web.Controllers
{
    // Controlador encargado de gestionar las vistas de clientes
    public class ClientesController : Controller
    {

        private readonly ApiClient _apiClient;

        private readonly IConfiguration _configuration;

        private readonly string _baseUrl;

        public ClientesController(ApiClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _configuration = configuration;
            _baseUrl = _configuration["ApiSettings:BaseUrl"];
        }


        /* LISTADO Y BÚSQUEDA DE CLIENTES */

        // Mostrar y filtrar clientes por nombre
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(string? nombre)
        {

            // Obtener los clientes desde la API
            var clientes = await _apiClient.ObtenerClientesAsync();
            if (clientes == null)
            {
                clientes = new List<ClienteViewModel>();
            }

            // Aplicar el filtro si se indicó un nombre
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                clientes = clientes
                    .Where(c => c.NombreCompleto.Contains(
                        nombre.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Conservar el texto ingresado en el buscador
            ViewBag.NombreBuscado = nombre;

            // Enviar los clientes a la vista
            return View(clientes);
        }


        /* REGISTRO DE CLIENTES */

        // Mostrar el formulario de registro
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Recibir y validar el formulario de registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClienteViewModel model)
        {
            // Verificar las validaciones
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return View(model);
        }


        // Mostrar la información de un cliente específico
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Validar el identificador
            if (id <= 0)
            {
                return BadRequest();
            }

            // Buscar al cliente en los datos de demostración
            var clientes = await _apiClient.ObtenerClientesAsync();
            
            ClienteViewModel cliente = clientes
                .FirstOrDefault(c => c.Id == id);

            // Verificar que exista
            if (cliente == null)
            {
                return NotFound();
            }

            // Enviar el cliente a la vista
            return View(cliente);
        }



        // Mostrar el formulario con los datos del cliente
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Validar el identificador
            if (id <= 0)
            {
                return BadRequest();
            }

            // Buscar al cliente correspondiente
            var clientes = await _apiClient.ObtenerClientesAsync();
            
            ClienteViewModel cliente = clientes
                .FirstOrDefault(c => c.Id == id);

            // Verificar que exista
            if (cliente == null)
            {
                return NotFound();
            }

            // Mostrar el formulario de edición
            return View(cliente);
        }

        // Recibir y validar los cambios realizados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClienteViewModel model)
        {
            // Validar los identificadores
            if (id <= 0 || id != model.Id)
            {
                return BadRequest();
            }

            // Verificar las validaciones del formulario
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar a la API para actualizar el cliente
            var response = await _apiClient.ActualizarClienteAsync(id, model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo actualizar el cliente."
                );
                return View(model);
            }

            // Regresar al listado de clientes
            return RedirectToAction(nameof(Index));
        }


        // Mostrar la pantalla de confirmación
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Validar el identificador
            if (id <= 0)
            {
                return BadRequest();
            }

            // Buscar al cliente correspondiente
            var clientes = await _apiClient.ObtenerClientesAsync();
            
            ClienteViewModel cliente = clientes
                .FirstOrDefault(c => c.Id == id);

            // Verificar que exista
            if (cliente == null)
            {
                return NotFound();
            }

            // Mostrar la confirmación de eliminación
            return View(cliente);
        }

        // Recibir la confirmación de eliminación
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Validar el identificador
            if (id <= 0)
            {
                return BadRequest();
            }

            // Comprobar que el cliente exista en los datos temporales
            var clientes = await _apiClient.ObtenerClientesAsync();

            ClienteViewModel cliente = clientes
                .FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            var response = await _apiClient.EliminarClienteAsync(id);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo eliminar el cliente."
                );
                return View(cliente);
            }

            // Regresar al listado de clientes
            return RedirectToAction(nameof(Index));
        }
        
    }
}
