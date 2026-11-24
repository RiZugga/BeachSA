using BeachSA.Web.Models;
using BeachSA.Web.Services;
using BeachSA.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;


namespace BeachSA.Web.Controllers
{
    public class ReservacionController : Controller
    {
        // Variables
        private readonly ApiService apiService = null;

        private HttpClient client = null;

        public ReservacionController()
        {
            // Se instancia el servicio
            this.apiService = new ApiService();

            // Se inicia la sincronización con la API
            this.client = this.apiService.ApiBeachSAStart();
        }


        // Listado de reservaciones
        public async Task<IActionResult> Index()
        {
            List<Reservacion> reservaList = new List<Reservacion>();

            HttpResponseMessage responseMessage = null;

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var token = User.FindFirst("AccessToken")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = "No se encontró el token de autenticación.";
                return RedirectToAction("Index", "Reservacion");
            }
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            // Consultar las reservaciones
            responseMessage = await this.client.GetAsync("Reservas/MyList");

            // Validación de respuesta
            if (responseMessage.IsSuccessStatusCode)
            {
                var result = await responseMessage.Content.ReadAsStringAsync();

                reservaList = JsonConvert.DeserializeObject<List<Reservacion>>(result);
            }
            else
            {
                TempData["ErrorMessage"] = "Error al intentar consultar las reservaciones.";
            }

            return View(reservaList);
        }


        // Mostrar formulario de registro
        [HttpGet]
        public async Task<IActionResult> Create(int paqueteId)
        {
            if (paqueteId <= 0)
            {
                return RedirectToAction("Index", "Paquete");
            }

            HttpResponseMessage response = await this.client.GetAsync($"paquetes/{paqueteId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "No se encontró el paquete seleccionado.";
                return RedirectToAction("Index", "Paquete");
            }

            var result = await response.Content.ReadAsStringAsync();

            Paquete paquete = JsonConvert.DeserializeObject<Paquete>(result);

            if (paquete == null || !paquete.Activo)
            {
                TempData["ErrorMessage"] = "El paquete seleccionado no está disponible.";
                return RedirectToAction("Index", "Paquete");
            }

            ViewBag.Paquete = paquete;

            Reservacion reservacion = new Reservacion
            {
                PaqueteId = paquete.Id
            };

            return View(reservacion);
        }


        // Mostrar pantalla de cotización
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cotizar(Reservacion temp)
        {
            if (temp == null ||
                temp.PaqueteId <= 0 ||
                temp.CantidadPersonas <= 0 ||
                temp.CantidadNoches <= 0)
            {
                TempData["ErrorMessage"] = "Los datos de la reservación no son válidos.";
                return RedirectToAction("Index", "Paquete");
            }

            // Consultar el paquete seleccionado
            HttpResponseMessage response =
                await this.client.GetAsync($"paquetes/{temp.PaqueteId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "No se encontró el paquete seleccionado.";
                return RedirectToAction("Index", "Paquete");
            }

            var result = await response.Content.ReadAsStringAsync();

            Paquete paquete = JsonConvert.DeserializeObject<Paquete>(result);

            if (paquete == null || !paquete.Activo)
            {
                TempData["ErrorMessage"] = "El paquete seleccionado no está disponible.";
                return RedirectToAction("Index", "Paquete");
            }

            CotizacionViewModel modelo = new CotizacionViewModel
            {
                PaqueteId = paquete.Id,
                PaqueteNombre = paquete.Nombre,
                CantidadPersonas = temp.CantidadPersonas,
                CantidadNoches = temp.CantidadNoches
            };

            return View("Cotizar", modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MandarPDF(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID de reservación no válido.";
                return RedirectToAction("Index", "Reservacion");
            }

            var token = User.FindFirst("AccessToken")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = "No se encontró el token de autenticación.";
                return RedirectToAction("Index", "Reservacion");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, $"Reservas/EnviarPDF?id={id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await this.client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "No se pudo enviar el PDF de la reservación.";
                return RedirectToAction("Index", "Reservacion");
            }

            TempData["MensajeExito"] = "El PDF se envió correctamente al correo del cliente.";
            return RedirectToAction("Index", "Reservacion");
        }

        // Calcular cotización mediante la API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalcularCotizacion(CotizacionViewModel temp)
        {
            if (temp == null)
            {
                TempData["ErrorMessage"] = "Los datos de la cotización no son válidos.";
                return RedirectToAction("Index", "Paquete");
            }

            if (temp.PaqueteId <= 0 ||
                temp.CantidadPersonas <= 0 ||
                temp.CantidadNoches <= 0 ||
                string.IsNullOrWhiteSpace(temp.FormaPago))
            {
                TempData["ErrorMessage"] = "Debe completar los datos de la cotización.";
                return View("Cotizar", temp);
            }

            var solicitud = new
            {
                PaqueteId = temp.PaqueteId,
                CantidadPersonas = temp.CantidadPersonas,
                CantidadNoches = temp.CantidadNoches,
                FormaPago = temp.FormaPago
            };

            HttpResponseMessage response =
                await this.client.PostAsJsonAsync("Reservas/Cotizar", solicitud);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                temp.Resultado = JsonConvert.DeserializeObject<Cotizacion>(result);

                if (temp.Resultado == null)
                {
                    TempData["ErrorMessage"] = "No se pudo interpretar la cotización recibida.";
                    return View("Cotizar", temp);
                }

                temp.CotizacionCalculada = true;
                temp.PaqueteNombre = temp.Resultado.PaqueteNombre;

                return View("Cotizar", temp);
            }

            TempData["ErrorMessage"] =
                $"Error al calcular la cotización: {await response.Content.ReadAsStringAsync()}";

            return View("Cotizar", temp);
        }


        // Confirmar y guardar la reservación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(CotizacionViewModel temp)
        {
            if (temp == null ||
                temp.PaqueteId <= 0 ||
                temp.CantidadPersonas <= 0 ||
                temp.CantidadNoches <= 0 ||
                string.IsNullOrWhiteSpace(temp.FormaPago))
            {
                TempData["ErrorMessage"] = "Los datos de la reservación no son válidos.";
                return View("Cotizar", temp);
            }

            if (!temp.CotizacionCalculada)
            {
                TempData["ErrorMessage"] = "Primero debe calcular la cotización.";
                return View("Cotizar", temp);
            }

            // 1. Obtener el JWT de la cookie de autenticación
            var token = User.FindFirst("AccessToken")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = "No se encontró el token de autenticación.";
                return View("Cotizar", temp);
            }

            // 2. Obtener el correo del usuario
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(correo))
            {
                TempData["ErrorMessage"] =
                    "No se encontró el correo del usuario autenticado.";

                return View("Cotizar", temp);
            }

            // 3. Consultar el cliente mediante su correo
            var respuestaCliente = await client.GetAsync(
                $"clientes/por-correo?correo={Uri.EscapeDataString(correo)}");

            if (!respuestaCliente.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "No se encontró un cliente asociado al correo del usuario.";

                return View("Cotizar", temp);
            }

            var contenidoCliente =
                await respuestaCliente.Content.ReadAsStringAsync();

            var cliente =
                JsonConvert.DeserializeObject<ClienteViewModel>(contenidoCliente);

            if (cliente == null || cliente.Id <= 0)
            {
                TempData["ErrorMessage"] = "No se pudo identificar al cliente.";
                return View("Cotizar", temp);
            }

            // 4. Construir los datos para crear la reservación
            var reservacionDTO = new
            {
                ClienteId = cliente.Id,
                PaqueteId = temp.PaqueteId,
                CantidadPersonas = temp.CantidadPersonas,
                CantidadNoches = temp.CantidadNoches,
                FormaPago = temp.FormaPago,
                NumeroCheque = temp.NumeroCheque,
                Banco = temp.Banco,
                Activo = true
            };

            // 5. Enviar la reservación a la API con el JWT
            using var request = new HttpRequestMessage(
                HttpMethod.Post, "Reservas/Create");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(reservacionDTO);

            var respuestaReserva = await client.SendAsync(request);

            // 6. Procesar la respuesta
            if (respuestaReserva.IsSuccessStatusCode)
            {
                TempData["MensajeExito"] =
                    "¡La reservación se ha registrado correctamente!";

                return View("Cotizar", temp);
            }

            var error = await respuestaReserva.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                "No se pudo guardar la reservación: " + error;

            return View("Cotizar", temp);
        }



        // Consultar información de una reservación
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            Reservacion temp = new Reservacion();

            try
            {
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "El valor del Id no es correcto.";

                    return RedirectToAction("Index");
                }

                HttpResponseMessage http = await this.client.GetAsync($"Reservas/SearchID?id={id}");

                if (http.IsSuccessStatusCode)
                {
                    var result = await http.Content.ReadAsStringAsync();

                    temp = JsonConvert.DeserializeObject<Reservacion>(result);
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al consultar la reservación.";

                    return RedirectToAction("Index");
                }

                return View(temp);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Consultar reservación para editar
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                Reservacion temp = new Reservacion();

                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "El valor del Id no es correcto.";

                    return RedirectToAction("Index");
                }
                else
                {
                    // Buscar la reservación
                    HttpResponseMessage response = await this.client.GetAsync($"Reservas/SearchID?id={id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        temp = JsonConvert.DeserializeObject<Reservacion>(result);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "No se encontró la reservación solicitada.";

                        return RedirectToAction("Index");
                    }

                    return View(temp);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Modificar reservación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Reservacion temp)
        {
            try
            {
                if (temp == null)
                {
                    TempData["ErrorMessage"] = "No se permiten valores vacíos.";

                    return View(temp);
                }

                if (!ModelState.IsValid)
                {
                    return View(temp);
                }

                HttpResponseMessage http = await this.client.PutAsJsonAsync($"Reservas/Update?id={temp.Id}", temp);

                if (http.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "La reservación se modificó correctamente.";

                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = $"Error: {await http.Content.ReadAsStringAsync()}";

                    return View(temp);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public IActionResult CreatePago(int id)
        {
            if (id <= 0)
            {
                TempData["MensajeError"] = "El identificador de la reservación no es válido.";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/PagoReservacion/CreatePago.cshtml",
                new PagoReservacionViewModel { ReservacionId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePago(PagoReservacionViewModel pago)
        {
            const string vista = "~/Views/PagoReservacion/CreatePago.cshtml";

            if (!ModelState.IsValid)
            {
                return View(vista, pago);
            }

            try
            {
                var token = User.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrWhiteSpace(token))
                {
                    ModelState.AddModelError(string.Empty, "No se encontró el token de autenticación.");
                    return View(vista, pago);
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, "Pagos/Aplicar");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(pago);
                using var http = await client.SendAsync(request);
                if (!http.IsSuccessStatusCode)
                {
                    var error = await http.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty,
                        string.IsNullOrWhiteSpace(error)
                            ? $"No se pudo registrar el pago (HTTP {(int)http.StatusCode})."
                            : $"No se pudo registrar el pago: {error}");
                    return View(vista, pago);
                }

                TempData["MensajeExito"] = "El pago se registró correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty,
                    "No fue posible comunicarse con el servicio de pagos.");
                return View(vista, pago);
            }
        }


        // Consultar reservación antes de cancelarla
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Reservacion temp = new Reservacion();

            try
            {
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "El valor del Id no es correcto.";

                    return RedirectToAction("Index");
                }

                HttpResponseMessage http = await this.client.GetAsync($"Reservas/SearchID?id={id}");

                if (http.IsSuccessStatusCode)
                {
                    var result = await http.Content.ReadAsStringAsync();

                    temp = JsonConvert.DeserializeObject<Reservacion>(result);
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al consultar la reservación.";

                    return RedirectToAction("Index");
                }

                return View(temp);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Cancelar reservación
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deleted(int id)
        {
            HttpResponseMessage http = await this.client.DeleteAsync($"Reservas/Delete?id={id}");

            if (http.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "La reservación se canceló correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al intentar cancelar la reservación.";
            }

            return RedirectToAction("Index");
        }

    }//Close controller
}// Close namespace