namespace proyecto.Dto
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
}
