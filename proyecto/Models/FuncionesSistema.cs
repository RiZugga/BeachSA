using System.Security.Claims;

namespace proyecto.Models
{
    public class Funcion
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Orden { get; set; }
    }

    public static class FuncionesSistema
    {
        // Códigos de función: se usan como nombre de política en [Authorize(Policy = ...)]
        public const string Clientes = "Clientes";
        public const string Paquetes = "Paquetes";
        public const string Reservaciones = "Reservaciones";
        public const string Usuarios = "Usuarios";
        public const string Auditoria = "Auditoria";

        // Catálogo de funciones (opciones del menú)
        public static readonly IReadOnlyList<Funcion> Todas = new List<Funcion>
        {
            new Funcion { Codigo = Clientes, Nombre = "Clientes", Orden = 1 },
            new Funcion { Codigo = Paquetes, Nombre = "Paquetes", Orden = 2 },
            new Funcion { Codigo = Reservaciones, Nombre = "Reservaciones", Orden = 3 },
            new Funcion { Codigo = Usuarios, Nombre = "Usuarios", Orden = 4 },
            new Funcion { Codigo = Auditoria, Nombre = "Auditoría", Orden = 5 }
        };

        // Funciones que tiene cada rol
        private static readonly Dictionary<string, string[]> FuncionesPorRol =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { RolesSistema.Administrador, new[] { Clientes, Paquetes, Reservaciones, Usuarios, Auditoria } },
                { RolesSistema.Regular, new[] { Clientes, Paquetes, Reservaciones } }
            };

        public static bool RolExiste(string? rol)
        {
            return !string.IsNullOrWhiteSpace(rol) && FuncionesPorRol.ContainsKey(rol);
        }

        public static IReadOnlyList<Funcion> ObtenerFunciones(string? rol)
        {
            if (!RolExiste(rol))
            {
                return new List<Funcion>();
            }

            var codigos = FuncionesPorRol[rol!];
            return Todas.Where(f => codigos.Contains(f.Codigo)).OrderBy(f => f.Orden).ToList();
        }

        public static bool RolTieneFuncion(string? rol, string codigoFuncion)
        {
            return RolExiste(rol) && FuncionesPorRol[rol!].Contains(codigoFuncion);
        }

        public static string? ObtenerRol(ClaimsPrincipal usuario)
        {
            return usuario.FindFirstValue(ClaimTypes.Role);
        }
    }
}
