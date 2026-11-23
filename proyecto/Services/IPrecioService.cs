using proyecto.Dto;
using proyecto.Models;

namespace proyecto.Services
{
    public interface IPrecioService
    {
        /// <summary>
        /// Calcula subtotal, descuento, IVA, totales, prima, saldo y mensualidad.
        /// No consulta la base de datos ni servicios externos: recibe el tipo de cambio.
        /// Lanza ArgumentException (mensaje en español) si algún dato es inválido.
        /// </summary>
        CotizacionDTO Calcular(Paquete paquete, int cantidadPersonas, int cantidadNoches, string formaPago, decimal tipoCambio);
    }
}
