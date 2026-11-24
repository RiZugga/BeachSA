using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using proyecto.Dto;
using proyecto.Models;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace proyecto.Services
{
    public class ReservacionCorreoService : IReservacionCorreoService
    {
        private readonly DbContextBank _context;
        private readonly IConfiguration _configuration;

        public ReservacionCorreoService(
            DbContextBank context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        // 1. Buscar la reservación y generar el PDF
        public async Task<byte[]> GenerarPdfAsync(int reservacionId)
        {
            var reservacion = await _context.Reservaciones
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r =>
                    r.Id == reservacionId && r.Activo);

            if (reservacion == null)
            {
                throw new KeyNotFoundException(
                    "No se encontró la reservación solicitada.");
            }

            if (reservacion.Cliente == null)
            {
                throw new InvalidOperationException(
                    "La reservación no tiene un cliente asociado.");
            }

            var cliente = reservacion.Cliente;

            // Generar el documento
            byte[] pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header()
                        .Text("COMPROBANTE DE RESERVACIÓN")
                        .Bold()
                        .FontSize(20)
                        .FontColor(Colors.Blue.Darken2);

                    page.Header()
                        .Text("Sistema de Reservaciones BeachSA")
                        .Bold()
                        .FontSize(16)
                        .FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(12);

                        column.Item().Text("INFORMACIÓN DEL CLIENTE")
                            .Bold().FontSize(14);

                        column.Item().Text($"Nombre: {cliente.NombreCompleto}");
                        column.Item().Text($"Cédula: {cliente.Cedula}");
                        column.Item().Text($"Teléfono: {cliente.Telefono}");
                        column.Item().Text($"Correo: {cliente.Email}");
                        column.Item().Text($"Dirección: {cliente.Direccion}");

                        column.Item().PaddingTop(15)
                            .Text("DETALLES DE LA RESERVACIÓN")
                            .Bold().FontSize(14);

                        column.Item().Text($"Número de reservación: {reservacion.Id}");
                        column.Item().Text($"Paquete: {reservacion.PaqueteId}");
                        column.Item().Text($"Personas: {reservacion.CantidadPersonas}");
                        column.Item().Text($"Noches: {reservacion.CantidadNoches}");
                        column.Item().Text($"Forma de pago: {reservacion.FormaPago}");

                        if (!string.IsNullOrWhiteSpace(reservacion.NumeroCheque))
                        {
                            column.Item().Text(
                                $"Número de cheque: {reservacion.NumeroCheque}");
                        }

                        if (!string.IsNullOrWhiteSpace(reservacion.Banco))
                        {
                            column.Item().Text($"Banco: {reservacion.Banco}");
                        }

                        column.Item().PaddingTop(15)
                            .Text("RESUMEN DE COSTOS")
                            .Bold().FontSize(14);

                        column.Item().Text(
                            $"Precio por persona/noche: ${reservacion.PrecioPersonaNoche:N2}");

                        column.Item().Text(
                            $"Subtotal: ${reservacion.Subtotal:N2}");

                        column.Item().Text(
                            $"Descuento: ${reservacion.MontoDescuento:N2}");

                        column.Item().Text(
                            $"IVA: ${reservacion.MontoIva:N2}");

                        column.Item().Text(
                            $"Total en dólares: ${reservacion.TotalUsd:N2}")
                            .Bold();

                        column.Item().Text(
                            $"Tipo de cambio: ₡{reservacion.TipoCambio:N4}");

                        column.Item().Text(
                            $"Total en colones: ₡{reservacion.TotalColones:N2}")
                            .Bold();

                        column.Item().PaddingTop(10)
                            .Text($"Prima: ${reservacion.MontoPrima:N2}");

                        column.Item().Text(
                            $"Saldo pendiente: ${reservacion.Saldo:N2}");

                        column.Item().Text(
                            $"Cantidad de mensualidades: {reservacion.CantidadMensualidades}");

                        column.Item().Text(
                            $"Monto por mensualidad: ${reservacion.MontoMensualidad:N2}");

                        column.Item().PaddingTop(20)
                            .Text(
                                $"Fecha de reservación: {reservacion.FechaCreacion:dd/MM/yyyy}");
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Gracias por confiar en nuestro servicio.")
                        .FontSize(10);
                });
            }).GeneratePdf();

            return pdf;
        }

        // 2. Enviar el PDF por correo
        public async Task EnviarPdfAsync(int reservacionId)
        {
            await EnviarCorreoInternoAsync(reservacionId);
        }

        // 3. Reenviar el PDF por correo
        public async Task ReenviarPdfAsync(int reservacionId)
        {
            await EnviarCorreoInternoAsync(reservacionId);
        }

        // Método privado compartido por enviar y reenviar
        private async Task EnviarCorreoInternoAsync(int reservacionId)
        {
            var reservacion = await _context.Reservaciones
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r =>
                    r.Id == reservacionId && r.Activo);

            if (reservacion == null)
            {
                throw new KeyNotFoundException(
                    "No se encontró la reservación.");
            }

            if (reservacion.Cliente == null ||
                string.IsNullOrWhiteSpace(reservacion.Cliente.Email))
            {
                throw new InvalidOperationException(
                    "El cliente no tiene un correo electrónico válido.");
            }

            // Generar PDF
            byte[] pdf = await GenerarPdfAsync(reservacionId);

            // Obtener configuración SMTP
            var emailSettings = _configuration.GetSection("EmailSettings");

            string smtpServer = emailSettings["SmtpServer"]
                ?? throw new InvalidOperationException("Falta SmtpServer.");

            int port = int.Parse(emailSettings["Port"] ?? "587");

            string senderName = emailSettings["SenderName"]
                ?? throw new InvalidOperationException("Falta SenderName.");

            string senderEmail = emailSettings["SenderEmail"]
                ?? throw new InvalidOperationException("Falta SenderEmail.");

            string username = emailSettings["Username"]
                ?? throw new InvalidOperationException("Falta Username.");

            string password = emailSettings["Password"]
                ?? throw new InvalidOperationException("Falta Password.");

            // Construir mensaje
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(senderName, senderEmail));

            message.To.Add(
                new MailboxAddress(
                    reservacion.Cliente.NombreCompleto,
                    reservacion.Cliente.Email));

            message.Subject = $"Comprobante de reservación #{reservacion.Id}";

            var body = new BodyBuilder
            {
                TextBody =
                    $"Estimado/a {reservacion.Cliente.NombreCompleto},\n\n" +
                    $"Adjuntamos el comprobante de su reservación " +
                    $"número {reservacion.Id}.\n\n" +
                    "Gracias por confiar en nuestro servicio." +
                    "BeachSA"
            };

            body.Attachments.Add(
                $"Reservacion_{reservacion.Id}.pdf",
                pdf,
                ContentType.Parse("application/pdf"));

            message.Body = body.ToMessageBody();

            // Enviar correo
            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync(
                    smtpServer,
                    port,
                    MailKit.Security.SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(username, password);

                await smtp.SendAsync(message);

                await smtp.DisconnectAsync(true);
            }

            // Actualizar estado solamente si el envío fue exitoso
            reservacion.CorreoEnviado = true;
            reservacion.FechaCorreo = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
