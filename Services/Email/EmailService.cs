using System.Text;
using System.Text.Json;
using fifa_backend.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace fifa_backend.Services.Email;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly BrevoSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        HttpClient httpClient,
        IOptions<BrevoSettings> settings,
        ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string email, string otp)
    {
        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            _logger.LogWarning("Brevo API Key is not configured. Email NOT sent. OTP Code: {Otp}", otp);
            return;
        }

        var payload = new
        {
            sender = new { name = _settings.SenderName, email = _settings.SenderEmail },
            to = new[] { new { email = email } },
            subject = "Your FIFA Fan Vote OTP Verification Code",
            htmlContent = $@"
                <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; max-width: 550px; margin: 20px auto; padding: 32px; border: 1px solid #e5e7eb; background-color: #ffffff; color: #111827;'>
                    <div style='border-bottom: 2px solid #2563eb; padding-bottom: 16px; margin-bottom: 24px;'>
                        <h2 style='color: #111827; margin: 0; font-size: 22px; font-weight: 800; letter-spacing: -0.02em;'>FIFA <span style='color: #2563eb;'>FAN VOTE</span></h2>
                    </div>
                    <p style='font-size: 16px; line-height: 1.5; margin: 0 0 16px 0; color: #374151;'>Hello,</p>
                    <p style='font-size: 15px; line-height: 1.6; margin: 0 0 24px 0; color: #4b5563;'>You requested a verification code to access your account on the FIFA Fan Voting Platform. Please use the following 6-digit One-Time Password (OTP):</p>
                    <div style='background-color: #f9fafb; border: 1px solid #e5e7eb; padding: 20px; text-align: center; margin: 24px 0;'>
                        <span style='font-family: monospace; font-size: 36px; font-weight: 800; letter-spacing: 6px; color: #2563eb;'>{otp}</span>
                    </div>
                    <p style='font-size: 14px; line-height: 1.6; margin: 24px 0 0 0; color: #6b7280;'>This code is valid for <strong>5 minutes</strong>. For security reasons, please do not share this code with anyone.</p>
                    <div style='border-top: 1px solid #e5e7eb; margin-top: 32px; padding-top: 20px; font-size: 12px; color: #9ca3af; text-align: center;'>
                        <p style='margin: 0 0 4px 0;'>FIFA Fan Voting Platform</p>
                        <p style='margin: 0;'>Secure fan polling services</p>
                    </div>
                </div>"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", _settings.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email via Brevo. Status: {StatusCode}, Response: {Response}", response.StatusCode, responseBody);
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("OTP email successfully sent to {Email}. Response: {Response}", email, responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending OTP email to {Email}", email);
        }
    }

    public async Task SendResultsEmailAsync(string email, string userName, string sessionTitle, string userVotedTeamName, List<string> winners)
    {
        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            _logger.LogWarning("Brevo API Key is not configured. Results email NOT sent to {Email}.", email);
            return;
        }

        var winnersHtml = string.Join("", winners.Select(w => $"<li style='font-size: 15px; margin-bottom: 8px; color: #111827;'>🏆 <strong>{w}</strong></li>"));

        var payload = new
        {
            sender = new { name = _settings.SenderName, email = _settings.SenderEmail },
            to = new[] { new { email = email } },
            subject = $"Results Published: {sessionTitle}",
            htmlContent = $@"
                <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; max-width: 550px; margin: 20px auto; padding: 32px; border: 1px solid #e5e7eb; background-color: #ffffff; color: #111827;'>
                    <div style='border-bottom: 2px solid #2563eb; padding-bottom: 16px; margin-bottom: 24px;'>
                        <h2 style='color: #111827; margin: 0; font-size: 22px; font-weight: 800; letter-spacing: -0.02em;'>FIFA <span style='color: #2563eb;'>FAN VOTE</span></h2>
                    </div>
                    <p style='font-size: 16px; line-height: 1.5; margin: 0 0 16px 0; color: #374151;'>Hello {userName},</p>
                    <p style='font-size: 15px; line-height: 1.6; margin: 0 0 24px 0; color: #4b5563;'>The voting results for the session <strong>{sessionTitle}</strong> have been officially published by the administrator!</p>
                    
                    <div style='background-color: #f3f4f6; border-radius: 6px; padding: 16px; margin-bottom: 24px;'>
                        <p style='font-size: 14px; margin: 0 0 8px 0; color: #4b5563;'>Your Vote:</p>
                        <p style='font-size: 16px; font-weight: 700; margin: 0; color: #2563eb;'>🗳️ {userVotedTeamName}</p>
                    </div>

                    <h3 style='font-size: 16px; font-weight: 700; margin: 0 0 12px 0; color: #111827; border-bottom: 1px solid #e5e7eb; padding-bottom: 8px;'>Winners:</h3>
                    <ul style='padding-left: 20px; margin: 0 0 24px 0;'>
                        {winnersHtml}
                    </ul>

                    <p style='font-size: 15px; line-height: 1.6; margin: 0 0 24px 0; color: #4b5563;'>You can view the full interactive results, charts, and details by logging into your profile on the FIFA Fan Voting Platform.</p>
                    
                    <div style='text-align: center; margin: 32px 0;'>
                        <a href='http://localhost:4200/profile' style='background-color: #2563eb; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: 600; font-size: 15px; display: inline-block;'>View My Votes & Results</a>
                    </div>

                    <div style='border-top: 1px solid #e5e7eb; margin-top: 32px; padding-top: 20px; font-size: 12px; color: #9ca3af; text-align: center;'>
                        <p style='margin: 0 0 4px 0;'>FIFA Fan Voting Platform</p>
                        <p style='margin: 0;'>Secure fan polling services</p>
                    </div>
                </div>"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", _settings.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send results email via Brevo to {Email}. Status: {StatusCode}, Response: {Response}", email, response.StatusCode, responseBody);
            }
            else
            {
                _logger.LogInformation("Results email successfully sent to {Email}.", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending results email to {Email}", email);
        }
    }
}
