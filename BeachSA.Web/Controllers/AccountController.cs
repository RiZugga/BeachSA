using BeachSA.Web.Services;
using BeachSA.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BeachSA.Web.Controllers
{
    public class AccountController : Controller
    {
        // Servicio para comunicarse con la API
        private readonly ApiClient _apiClient;

        // Configuración de la aplicación
        private readonly IConfiguration _configuration;

        // Constructor del controlador
        public AccountController(
            ApiClient apiClient,
            IConfiguration configuration)
        {
            _apiClient = apiClient;
            _configuration = configuration;
        }

        /* INICIO DE SESIÓN */

        // Mostrar la pantalla de inicio de sesión
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // Recibir los datos del formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Validar los campos del formulario
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Consultar la dirección configurada para la API
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            // Evitar solicitudes cuando todavía no existe Backend
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "El servicio de autenticación todavía no está disponible jejejeje."
                );

                return View(model);
            }

            try
            {
                // Enviar las credenciales a la API
                var resultado = await _apiClient.LoginAsync(model);

                // Verificar si la API rechazó las credenciales
                if (resultado == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Credenciales inválidas."
                    );

                    return View(model);
                }


                /* CREACIÓN DE LA SESIÓN DEL USUARIO */

                // Validar que la respuesta tenga un token y una fecha vigente
                if (string.IsNullOrWhiteSpace(resultado.Token) ||
                    resultado.ExpiresAtUtc <= DateTime.UtcNow)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "La respuesta de autenticación no es válida."
                    );

                    return View(model);
                }

                // Crear los datos de identidad del usuario
                var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, resultado.UserName),
    new Claim("FullName", resultado.FullName),

    // Guardar el JWT en el ticket protegido de autenticación
    new Claim("AccessToken", resultado.Token)
};

                // Agregar los roles asignados al usuario
                foreach (var rol in resultado.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }

                // Agregar las opciones de menú autorizadas
                foreach (var opcion in resultado.Menu)
                {
                    claims.Add(new Claim("Menu", System.Text.Json.JsonSerializer.Serialize(opcion)));
                }

                // Crear la identidad y el usuario autenticado
                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal = new ClaimsPrincipal(identity);

                // La sesión no debe superar el vencimiento del JWT
                var expiracion = resultado.ExpiresAtUtc;
                var limiteCookie = DateTimeOffset.UtcNow.AddMinutes(8);

                if (expiracion > limiteCookie.UtcDateTime)
                {
                    expiracion = limiteCookie.UtcDateTime;
                }

                // Configurar las propiedades de la autenticación
                var propiedades = new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = false,
                    ExpiresUtc = new DateTimeOffset(expiracion, TimeSpan.Zero)
                };

                // Crear la cookie de autenticación
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    propiedades
                );

                // Redirigir al Dashboard
                return RedirectToAction("Index", "Home");

            }
            catch (HttpRequestException)
            {
                // Manejar errores de comunicación con el Backend
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible comunicarse con el servicio de autenticación."
                );

                return View(model);
            }
        }

        /* ACCESO DENEGADO */

        // Mostrar la pantalla de acceso denegado
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /* CIERRE DE SESIÓN */

        // Eliminar la autenticación del usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            // Regresar al Login
            return RedirectToAction("Login", "Account");
        }
    }
}
