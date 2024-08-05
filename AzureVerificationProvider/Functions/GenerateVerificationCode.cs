using Azure.Messaging.ServiceBus;
using AzureVerificationProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureVerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, VerificationCodeService verificationCodeService)
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
    private readonly VerificationCodeService _verificationCodeService = verificationCodeService;

    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email_request", Connection = "ServiceBusConnection")]
    public async Task<string> Run(
        [ServiceBusTrigger("verification_request", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            var verificationRequest = _verificationCodeService.UnpackVerificationRequest(message);
            if (verificationRequest != null)
            {
                var code = _verificationCodeService.GenerateCode();
                if (!string.IsNullOrEmpty(code))
                {
                    if (await _verificationCodeService.SaveVerificationRequest(verificationRequest, code))
                    {
                        var emailRequest = _verificationCodeService.GenerateEmailRequest(verificationRequest, code);
                        if (emailRequest != null)
                        {
                            var payload = _verificationCodeService.GenerateServiceBusEmailRequest(emailRequest);
                            if (!string.IsNullOrEmpty(payload))
                            {
                                await messageActions.CompleteMessageAsync(message);
                                return payload;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: GenerateVerificationCode.Run :: {ex.Message}");
        }

        return null!;
    }

    
}
