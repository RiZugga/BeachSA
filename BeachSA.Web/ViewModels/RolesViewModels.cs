namespace BeachSA.Web.Models
{
    public class FuncionItemViewModel
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Orden { get; set; }
    }

    public class RolDetalleViewModel
    {
        public string rol { get; set; } = string.Empty;
        //public string Descripcion { get; set; } = string.Empty;
        public List<FuncionItemViewModel> Funciones { get; set; } = new();
    }

    public class RolesIndexViewModel
    {
        public string? Filtro { get; set; }
        public List<FuncionItemViewModel> CatalogoFunciones { get; set; } = new();
        public List<RolDetalleViewModel> Roles { get; set; } = new();
        public string? Error { get; set; }
    }
}