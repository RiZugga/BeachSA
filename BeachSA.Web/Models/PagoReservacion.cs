

namespace BeachSA.Web.Models
{
    public class PagoReservacion
    {
        public int Id { get; set; }


        public int ReservacionId { get; set; }


        public int UsuarioId { get; set; }


        public int NumeroMensualidad { get; set; }
        
        public decimal Monto { get; set; }

        public string Moneda { get; set; } = string.Empty;

        public decimal TipoCambioAplicado { get; set; }

        // Equivalencia del pago en dólares para actualizar el saldo
        public decimal MontoUsd { get; set; }
        public DateTime FechaPago { get; set; }
    }
}