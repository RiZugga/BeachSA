
namespace proyecto.Dto
{
    public class PagoResponseDTO
    {
        public int Id { get; set; }

        public int ReservacionId { get; set; }

        public int NumeroMensualidad { get; set; }

        public decimal Monto { get; set; }

        public string Moneda { get; set; } = string.Empty;

        public decimal TipoCambioAplicado { get; set; }

        public decimal MontoUsd { get; set; }

        public DateTime FechaPago { get; set; }

        public int UsuarioId { get; set; }

        public decimal SaldoRestante { get; set; }
    }
}
