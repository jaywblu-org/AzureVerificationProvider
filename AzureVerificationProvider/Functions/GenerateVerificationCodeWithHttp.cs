using AzureVerificationProvider.Services;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureVerificationProvider.Functions;

public class GenerateVerificationCodeWithHttp
{
    private readonly ILogger<GenerateVerificationCodeWithHttp> _logger;
    private readonly VerificationCodeService _verificationCodeService;

    public GenerateVerificationCodeWithHttp(ILogger<GenerateVerificationCodeWithHttp> logger, VerificationCodeService verificationCodeService)
    {
        _logger = logger;
        _verificationCodeService = verificationCodeService;
    }

    [Function("GenerateVerificationCodeWithHttp")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var verificationRequest = await _verificationCodeService.UnpackHttpVerificationRequest(req);
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
                                return new OkResult();
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: GenerateVerificationCodeWithHttp.Run :: {ex.Message}");
        }

        return new BadRequestResult();
    }
}
