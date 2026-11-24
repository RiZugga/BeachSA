using BeachSA.Web.Services;
using BeachSA.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BeachSA.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUsuarioApiService _usuarioApi;

        public UsuariosController(IUsuarioApiService usuarioApi)
        {
            _usuarioApi = usuarioApi;
        }

        public async Task<IActionResult> Index(string? filtro)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var (ok, data, error) = await _usuarioApi.ListarAsync(filtro);

            if (!ok || data == null)
            {
                TempData["MensajeError"] = error ?? "No se pudieron cargar los usuarios desde la API.";
                return View(new UsuarioIndexViewModel
                {
                    Filtro = filtro,
                    Usuarios = new List<UsuarioItemViewModel>()
                });
            }

            var lista = data.Select(u => new UsuarioItemViewModel
            {
                Id = u.Id,
                NombreUsuario = u.NombreUsuario,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email,
                Rol = u.Rol,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion
            });

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var term = filtro.Trim().ToLowerInvariant();
                lista = lista.Where(u =>
                    u.NombreUsuario.ToLowerInvariant().Contains(term) ||
                    u.NombreCompleto.ToLowerInvariant().Contains(term) ||
                    u.Email.ToLowerInvariant().Contains(term));
            }

            var vm = new UsuarioIndexViewModel
            {
                Filtro = filtro,
                Usuarios = lista.OrderBy(u => u.NombreUsuario).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new UsuarioCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (ok, error) = await _usuarioApi.CrearAsync(model);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error ?? "No se pudo registrar el usuario.");
                return View(model);
            }

            TempData["MensajeExito"] = "Usuario registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var (ok, usuario, error) = await _usuarioApi.ObtenerPorIdAsync(id);
            if (!ok || usuario == null)
            {
                TempData["MensajeError"] = error ?? "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(new UsuarioEditViewModel
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Activo = usuario.Activo
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(UsuarioEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (ok, error) = await _usuarioApi.EditarAsync(model);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error ?? "No se pudo actualizar el usuario.");
                return View(model);
            }

            TempData["MensajeExito"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AsignarRol(int id)
        {
            var (ok, usuario, error) = await _usuarioApi.ObtenerPorIdAsync(id);
            if (!ok || usuario == null)
            {
                TempData["MensajeError"] = error ?? "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(new AsignarRolViewModel
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Rol = usuario.Rol
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(AsignarRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (ok, error) = await _usuarioApi.AsignarRolAsync(model);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error ?? "No se pudo asignar el rol.");
                return View(model);
            }

            TempData["MensajeExito"] = $"Rol actualizado a '{model.Rol}' para {model.NombreUsuario}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var (ok, error) = await _usuarioApi.EliminarAsync(id);
            if (!ok)
            {
                TempData["MensajeError"] = error ?? "No se pudo eliminar el usuario.";
                return RedirectToAction(nameof(Index));
            }

            TempData["MensajeExito"] = "Usuario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}