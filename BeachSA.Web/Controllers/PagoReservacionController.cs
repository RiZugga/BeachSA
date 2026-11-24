using BeachSA.Web.Models;
using BeachSA.Web.Services;
using BeachSA.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace BeachSA.Web.Controllers
{
    [Authorize]
    public class PagoReservacionController : Controller
    {
        private readonly ApiService apiService = null;
        private HttpClient client = null;

        public PagoReservacionController()
        {
            this.apiService = new ApiService();
            this.client = this.apiService.ApiBeachSAStart();
        }

        // Listado de pagos
        public async Task<IActionResult> Index()
        {
            List<PagoReservacion> pagosList = new List<PagoReservacion>();

            try
            {
                var token = User.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrWhiteSpace(token))
                {
                    TempData["ErrorMessage"] = "No se encontró el token de autenticación.";
                    return RedirectToAction("Index", "Home");
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, "Pagos/Historial");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var http = await client.SendAsync(request);

                if (http.IsSuccessStatusCode)
                {
                    var result = await http.Content.ReadAsStringAsync();
                    pagosList = JsonConvert.DeserializeObject<List<PagoReservacion>>(result);
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al intentar consultar los pagos.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los pagos: " + ex.Message;
            }

            return View(pagosList);
        }

        // Mostrar formulario para crear pago
        [HttpGet]
        public async Task<IActionResult> Create(int reservacionId)
        {
            if (reservacionId <= 0)
            {
                TempData["ErrorMessage"] = "El identificador de la reservación no es válido.";
                return RedirectToAction("Index", "Reservacion");
            }

            try
            {
                var token = User.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrWhiteSpace(token))
                {
                    TempData["ErrorMessage"] = "No se encontró el token de autenticación.";
                    return RedirectToAction("Index", "Reservacion");
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, $"Reservas/SearchID?id={reservacionId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var http = await client.SendAsync(request);

                if (!http.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "No se encontró la reservación.";
                    return RedirectToAction("Index", "Reservacion");
                }

                var resultado = await http.Content.ReadAsStringAsync();
                var reservacion = JsonConvert.DeserializeObject<Reservacion>(resultado);

                var pagoViewModel = new PagoReservacionViewModel
                {
                    ReservacionId = reservacionId,
                    Reservacion = reservacion
                };

                return View(pagoViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la reservación: " + ex.Message;
                return RedirectToAction("Index", "Reservacion");
            }
        }

        // Guardar pago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PagoReservacionViewModel pago)
        {
            if (!ModelState.IsValid)
            {
                return View(pago);
            }

            try
            {
                var token = User.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrWhiteSpace(token))
                {
                    ModelState.AddModelError(string.Empty, "No se encontró el token de autenticación.");
                    return View(pago);
                }

                var pagoDTO = new
                {
                    pago.ReservacionId,
                    pago.NumeroMensualidad,
                    pago.Monto,
                    pago.Moneda
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, "Pagos/Aplicar");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(pagoDTO);

                using var http = await client.SendAsync(request);
                if (!http.IsSuccessStatusCode)
                {
                    var error = await http.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"No se pudo registrar el pago: {error}");
                    return View(pago);
                }

                TempData["SuccessMessage"] = "El pago se registró correctamente.";
                return RedirectToAction("Details", "Reservacion", new { id = pago.ReservacionId });
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty,
                    "No fue posible comunicarse con el servicio de pagos: " + ex.Message);
                return View(pago);
            }
        }

    }
}
