using Azure.Messaging.ServiceBus;
using AzureVerificationProvider.Data.Contexts;
using AzureVerificationProvider.Data.Enttities;
using AzureVerificationProvider.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureVerificationProvider.Services;

public class VerificationCodeService(ILogger<VerificationCodeService> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<VerificationCodeService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public string GenerateServiceBusEmailRequest(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: GenerateVerificationCode.GenerateServiceBusEmailRequest :: {ex.Message}");
        }

        return null!;
    }

    public async Task<bool> SaveVerificationRequest(VerificationRequet verificationRequest, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();

            var existing = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == verificationRequest.Email);
            if (existing != null)
            {
                existing.Code = code;
                existing.ExpiryDate = DateTime.Now.AddMinutes(5);
                context.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                context.VerificationRequests.Add(new VerificationRequestEntity()
                {
                    Email = verificationRequest.Email,
                    Code = code
                });
            }

            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: GenerateVerificationCode.SaveVerificationRequest :: {ex.Message}");
        }

        return false;
    }

    public EmailRequest GenerateEmailRequest(VerificationRequet verificationRequest, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequest()
                {
                    To = verificationRequest.Email,
                    Subject = $"Verification Code: {code}",
                    HtmlBody = $"<html><body><h1 style='text-align: center;'>Your verification code</h1><p style='text-align: center; margin-top: 24px;'>{code}</p></body></html>",
                    PlainText = $"Your verification code is {code}"
                };

                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: GenerateVerificationCode.GenerateEmailRequest :: {ex.Message}");
        }

        return null!;
    }

    public VerificationRequet UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var verificationRequest = JsonConvert.DeserializeObject<VerificationRequet>(message.Body.ToString());
            if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
            {
                return verificationRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: GenerateVerificationCode.UnpackVerificationRequest :: {ex.Message}");
        }

        return null!;
    }

    public string GenerateCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(100000, 999999);
            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: GenerateVerificationCode.GenerateCode :: {ex.Message}");
        }

        return null!;
    }
}
