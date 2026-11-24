using BeachSA.Web.Models;
using BeachSA.Web.Services;
using BeachSA.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace BeachSA.Web.Controllers
{
    [Authorize]
    public class PaqueteController : Controller
    {

        // Variables
        private readonly ApiService apiService = null;

        private HttpClient client = null;

        public PaqueteController()
        {
            // Se instancia el servicio
            this.apiService = new ApiService();

            // Se inicia la sincronización con la API
            this.client = this.apiService.ApiBeachSAStart();
        }


        // Listado y búsqueda de paquetes
        public async Task<IActionResult> Index(string pName)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "User");
            }

            if (!this.SetToken())
            {
                return RedirectToAction("Login", "Account");
            }

            List<Paquete> paqueteList = new List<Paquete>();
            RolDetalleViewModel rol = new RolDetalleViewModel();

            HttpResponseMessage responseMessage = null;

            if (pName == null)
            {
                responseMessage = await this.client.GetAsync("paquetes");
            }
            else
            {
                responseMessage = await this.client.GetAsync($"paquetes/nombre/{pName}");
            }

            // Validación de respuesta
            if (responseMessage.IsSuccessStatusCode)
            {
                var result = await responseMessage.Content.ReadAsStringAsync();
                if (pName == null)
                {
                    paqueteList = JsonConvert.DeserializeObject<List<Paquete>>(result);
                }
                else
                {
                    Paquete paquete = JsonConvert.DeserializeObject<Paquete>(result);
                    if (paquete != null)
                    {
                        paqueteList.Add(paquete);
                    }
                }


            }
            else
            {
                TempData["ErrorMessage"] = "Error al intentar consultar los paquetes.";
            }

            HttpResponseMessage responseMenu = await this.client.GetAsync($"Menu");

            if (responseMenu.IsSuccessStatusCode)
            {
                var resultRol = await responseMenu.Content.ReadAsStringAsync();
                rol = JsonConvert.DeserializeObject<RolDetalleViewModel>(resultRol);
            }
            else
            {
                TempData["ErrorMessage"] = "Error al intentar consultar los rol.";
            }
            Console.WriteLine(rol.rol);

            if (rol.rol == "Regular")
            {
                return View("IndexCliente", paqueteList);
            }


            return View("Index", paqueteList);//Colocar dentro la vista de usuario
        }


        // Mostrar formulario de registro
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // Registrar paquete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Paquete temp)
        {
            // Validaciones
            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se permiten valores vacíos.";

                return View(temp);
            }

            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            // Uso del endpoint
            HttpResponseMessage response = await this.client.PostAsJsonAsync<Paquete>("paquetes", temp);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "El paquete se registró correctamente.";

                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Error al intentar registrar el paquete. Verifique los datos.";

                return View(temp);
            }
        }


        // Consultar paquete para editar
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                Paquete temp = new Paquete();

                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "El valor del Id no es correcto.";

                    return RedirectToAction("Index");
                }
                else
                {
                    // Buscar el paquete
                    HttpResponseMessage response = await this.client.GetAsync($"paquetes/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        temp = JsonConvert.DeserializeObject<Paquete>(result);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "No se encontró el paquete solicitado.";

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


        // Modificar paquete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Paquete temp)
        {
            try
            {
                if (this.SetToken() == false)
                {
                    return RedirectToAction("Login", "User");
                }

                if (temp == null)
                {
                    TempData["ErrorMessage"] = "No se permiten valores vacíos.";

                    return View(temp);
                }
                else
                {
                    HttpResponseMessage http = await this.client.PutAsJsonAsync<Paquete>($"paquetes/{temp.Id}", temp);

                    if (http.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "El paquete se modificó correctamente.";

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Error: {await http.Content.ReadAsStringAsync()}";

                        return View(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Consultar información de un paquete
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            Paquete temp = new Paquete();

            try
            {
                HttpResponseMessage http = await this.client.GetAsync($"paquetes/{id}");

                if (http.IsSuccessStatusCode)
                {
                    var result = await http.Content.ReadAsStringAsync();

                    temp = JsonConvert.DeserializeObject<Paquete>(result);
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al consultar el paquete.";

                    return RedirectToAction("Index");
                }

                return View(temp);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Consultar paquete antes de desactivarlo
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Paquete temp = new Paquete();

            try
            {
                HttpResponseMessage http = await this.client.GetAsync($"paquetes/{id}");

                if (http.IsSuccessStatusCode)
                {
                    var result = await http.Content.ReadAsStringAsync();

                    temp = JsonConvert.DeserializeObject<Paquete>(result);
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al consultar el paquete.";

                    return RedirectToAction("Index");
                }

                return View(temp);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Desactivar paquete
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deleted(int id)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            HttpResponseMessage http = await this.client.DeleteAsync($"paquetes/{id}");

            if (http.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "El paquete se desactivó correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al intentar desactivar el paquete.";
            }

            return RedirectToAction("Index");
        }


        // Obtener el token de autenticación
        private AuthenticationHeaderValue? AuthorizationValue()
        {
            try
            {
                string? token = User.FindFirst("AccessToken")?.Value;

                return string.IsNullOrWhiteSpace(token)
                    ? null
                    : new AuthenticationHeaderValue("Bearer", token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Establecer el token en las solicitudes HTTP
        private bool SetToken()
        {
            try
            {
                bool result = false;

                this.client.DefaultRequestHeaders.Authorization = this.AuthorizationValue();

                if (this.client.DefaultRequestHeaders.Authorization == null)
                {
                    HttpContext.Session.Clear();
                    HttpContext.SignOutAsync();

                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }//Close controller 
}// Close Space
