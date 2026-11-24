using Microsoft.AspNetCore.Mvc;
using BeachSA.Web.Models;
using BeachSA.Web.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace BeachSA.Web.Controllers
{
    [Authorize]
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

            // Consultar las reservaciones
            responseMessage = await this.client.GetAsync("Reservas/List");

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
                TempData["ErrorMessage"] =
                    "Los datos de la reservación no son válidos.";

                return View("Cotizar", temp);
            }

            if (!temp.CotizacionCalculada)
            {
                TempData["ErrorMessage"] =
                    "Primero debe calcular la cotización.";

                return View("Cotizar", temp);
            }


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


        //// Obtener el token de autenticación
        //private AuthenticationHeaderValue AuthorizationValue()
        //{
        //    try
        //    {
        //        string varToken = HttpContext.Session.GetString("token");

        //        AuthenticationHeaderValue authentication = null;

        //        if (varToken != null)
        //        {
        //            if (varToken.Length > 0)
        //            {
        //                authentication = new AuthenticationHeaderValue("Bearer", varToken);
        //            }
        //        }

        //        return authentication;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        //// Establecer el token en las solicitudes HTTP
        //private bool SetToken()
        //{
        //    try
        //    {
        //        bool result = false;

        //        this.client.DefaultRequestHeaders.Authorization = this.AuthorizationValue();

        //        if (this.client.DefaultRequestHeaders.Authorization == null)
        //        {
        //            HttpContext.Session.Clear();
        //            HttpContext.SignOutAsync();

        //            result = false;
        //        }
        //        else
        //        {
        //            result = true;
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

    }//Close controller
}// Close namespace