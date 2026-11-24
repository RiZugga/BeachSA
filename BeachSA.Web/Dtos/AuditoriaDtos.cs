namespace BeachSA.Web.Dtos
{
    public class AuditoriaFiltroDto
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Accion { get; set; }
        public string? Entidad { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 20;
    }

    public class RegistroAuditoriaDto
    {
        public long Id { get; set; }
        public int? UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string? Entidad { get; set; }
        public string? EntidadId { get; set; }
        public string? Detalle { get; set; }
        public string? DireccionIp { get; set; }
    }

    public class AuditoriaResultadoDto
    {
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public List<RegistroAuditoriaDto> Items { get; set; } = new();
        public int TotalPaginas => TamanoPagina > 0 ? (int)Math.Ceiling((double)Total / TamanoPagina) : 1;
    }
}
