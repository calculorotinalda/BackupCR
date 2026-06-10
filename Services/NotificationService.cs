using System;
using System.Net;
using System.Net.Mail;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BackupCR.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient = new();

        public async Task SendNotificationAsync(
            string title, 
            string message, 
            string status, 
            string? smtpServer = null, 
            int? smtpPort = null, 
            string? smtpUser = null, 
            string? smtpPassword = null, 
            string? toEmail = null, 
            string? webhookUrl = null)
        {
            // 1. Email notification
            if (!string.IsNullOrEmpty(toEmail) && !string.IsNullOrEmpty(smtpServer))
            {
                try
                {
                    using var mail = new MailMessage();
                    mail.From = new MailAddress(smtpUser ?? "alerts@backupcr.com", "BackupCR Alert Manager");
                    mail.To.Add(toEmail);
                    mail.Subject = $"[{status.ToUpper()}] BackupCR: {title}";
                    mail.Body = $"Status: {status}\nData: {DateTime.Now}\nDetalhes:\n{message}";
                    mail.IsBodyHtml = false;

                    using var smtp = new SmtpClient(smtpServer, smtpPort ?? 587);
                    smtp.EnableSsl = true;
                    if (!string.IsNullOrEmpty(smtpUser))
                    {
                        smtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                    }
                    await smtp.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao enviar e-mail de alerta: {ex.Message}");
                }
            }

            // 2. Webhook notification
            if (!string.IsNullOrEmpty(webhookUrl))
            {
                try
                {
                    var payload = new
                    {
                        title = title,
                        message = message,
                        status = status,
                        timestamp = DateTime.UtcNow
                    };
                    var json = JsonSerializer.Serialize(payload);
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync(webhookUrl, content);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao enviar webhook: {ex.Message}");
                }
            }
        }
    }
}
