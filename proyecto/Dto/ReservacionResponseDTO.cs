namespace proyecto.Dto
{
    /// <summary>
    /// Reservación tal como se devuelve al consumir la API: incluye el cálculo
    /// guardado más los datos del cliente y el estado del correo.
    /// </summary>
    public class ReservacionResponseDTO : CotizacionDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteCedula { get; set; } = string.Empty;
        public int UsuarioId { get; set; }

        public string? NumeroCheque { get; set; }
        public string? Banco { get; set; }

        public bool CorreoEnviado { get; set; }
        public DateTime? FechaCorreo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
