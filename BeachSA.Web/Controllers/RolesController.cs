using BeachSA.Web.Models;
using BeachSA.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeachSA.Web.Controllers
{
    public class RolesController : Controller
    {
        private static readonly (string Nombre, string Descripcion)[] RolesConocidos =
        {
            ("Administrador", "Acceso completo a administración de usuarios, auditoría y módulos operativos."),
            ("Regular", "Acceso a módulos operativos principales (clientes, paquetes y reservaciones).")
        };

        private readonly RolApiService _rolApiService;

        public RolesController(RolApiService rolApiService)
        {
            _rolApiService = rolApiService;
        }

        public async Task<IActionResult> Index(string? filtro)
        {
            var vm = new RolesIndexViewModel
            {
                Filtro = filtro?.Trim()
            };

            var (catalogo, errorCatalogo) = await _rolApiService.ListarFuncionesAsync();
            if (errorCatalogo is not null)
            {
                vm.Error = errorCatalogo;
                return View(vm);
            }

            vm.CatalogoFunciones = catalogo;

            var rolesFiltrados = string.IsNullOrWhiteSpace(vm.Filtro)
                ? RolesConocidos
                : RolesConocidos.Where(r => r.Nombre.Contains(vm.Filtro, StringComparison.OrdinalIgnoreCase)
                                         || r.Descripcion.Contains(vm.Filtro, StringComparison.OrdinalIgnoreCase)).ToArray();

            foreach (var rol in rolesFiltrados)
            {
                var (funcionesRol, errorRol) = await _rolApiService.ObtenerFuncionesPorRolAsync(rol.Nombre);
                if (errorRol is not null)
                {
                    vm.Error = errorRol;
                    break;
                }

                vm.Roles.Add(new RolDetalleViewModel
                {
                    rol = rol.Nombre,
                    //Descripcion = rol.Descripcion,
                    Funciones = funcionesRol
                });
            }

            return View(vm);
        }
    }
}