using Microsoft.AspNetCore.Mvc;
using BeachSA.Web.ViewModels;
// Activar [Authorize] cuando la API de autenticación esté disponible.
// Las acciones de clientes deberán requerir una sesión válida.
using Microsoft.AspNetCore.Authorization;

namespace BeachSA.Web.Controllers
{
    // Controlador encargado de gestionar las vistas de clientes
    public class ClientesController : Controller
    {
        /* DATOS TEMPORALES PARA PRUEBAS */

        // Obtener clientes de demostración mientras no existe la API
        private static List<ClienteViewModel> ObtenerClientesDemo()
        {
            return new List<ClienteViewModel>
            {
                new ClienteViewModel
                {
                    Id = 1,
                    Cedula = "123456789",
                    TipoCedula = "Nacional",
                    NombreCompleto = "María Rodríguez",
                    Telefono = "88888888",
                    Email = "maria@ejemplo.com",
                    Direccion = "San José, Costa Rica",
                    Activo = true
                },

                new ClienteViewModel
                {
                    Id = 2,
                    Cedula = "987654321",
                    TipoCedula = "Nacional",
                    NombreCompleto = "Juan Pérez",
                    Telefono = "87777777",
                    Email = "juan@ejemplo.com",
                    Direccion = "Alajuela, Costa Rica",
                    Activo = true
                },

                new ClienteViewModel
                {
                    Id = 3,
                    Cedula = "456789123",
                    TipoCedula = "Nacional",
                    NombreCompleto = "María Fernández",
                    Telefono = "86666666",
                    Email = "mariaf@ejemplo.com",
                    Direccion = "Heredia, Costa Rica",
                    Activo = false
                }
            };
        }


        /* LISTADO Y BÚSQUEDA DE CLIENTES */

        // Mostrar y filtrar clientes por nombre
        [HttpGet]
        public IActionResult Index(string? nombre)
        {
            // Obtener los clientes temporales
            var clientes = ObtenerClientesDemo();

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

            // Registro pendiente de integración con la API
            ModelState.AddModelError(
                string.Empty,
                "El formulario es válido, pero el registro estará disponible cuando se integre la API. Bad Mine dijeron por ahí"
            );

            return View(model);
        }


        /* CONSULTA DE DETALLES DEL CLIENTE */

        // Mostrar la información de un cliente específico
        [HttpGet]
        public IActionResult Details(int id)
        {
            // Validar el identificador
            if (id <= 0)
            {
                return BadRequest();
            }

            // Buscar al cliente en los datos de demostración
            var cliente = ObtenerClientesDemo()
                .FirstOrDefault(c => c.Id == id);

            // Verificar que exista
            if (cliente == null)
            {
                return NotFound();
            }

            // Enviar el cliente a la vista
            return View(cliente);
        }


        /* EDICIÓN DE CLIENTES */

        // Mostrar el formulario con los datos del cliente
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Validar el identificador
            if (id <= 0)
            {
                return BadRequest();
            }

            // Buscar al cliente correspondiente
            var cliente = ObtenerClientesDemo()
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
        public IActionResult Edit(int id, ClienteViewModel model)
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

            // Actualización pendiente de integración con la API
            ModelState.AddModelError(
                string.Empty,
                "Los datos son válidos, pero la actualización estará disponible cuando se integre la API. Mala mía"
            );

            return View(model);
        }


        /* ELIMINACIÓN DE CLIENTES */

        // Mostrar la pantalla de confirmación
        [HttpGet]
        public IActionResult Delete(int id)
        {
            // Validar el identificador
            if (id <= 0)
            {
                return BadRequest();
            }

            // Buscar al cliente correspondiente
            var cliente = ObtenerClientesDemo()
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
        public IActionResult DeleteConfirmed(int id)
        {
            // Validar el identificador
            if (id <= 0)
            {
                return BadRequest();
            }

            // Comprobar que el cliente exista en los datos temporales
            var cliente = ObtenerClientesDemo()
                .FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            // Eliminación pendiente de integración con la API
            TempData["Mensaje"] =
                "La eliminación estará disponible cuando se integre la API. Una disculpa querido usuario, pero le tocará esperar";

            // Regresar al listado de clientes
            return RedirectToAction(nameof(Index));
        }
    }
}
