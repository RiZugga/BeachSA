namespace proyecto.Services
{
    /// <summary>
    /// Se lanza cuando ni el servicio principal ni el de respaldo pudieron
    /// entregar el tipo de cambio.
    /// </summary>
    public class TipoCambioNoDisponibleException : Exception
    {
        public TipoCambioNoDisponibleException()
            : base("No fue posible obtener el tipo de cambio. Intente de nuevo más tarde.")
        {
        }
    }
}
