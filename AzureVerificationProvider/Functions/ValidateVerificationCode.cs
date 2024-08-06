using AzureVerificationProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureVerificationProvider.Functions;

public class ValidateVerificationCode
{
    private readonly ILogger<ValidateVerificationCode> _logger;
    private readonly ValidateVerificationCodeService _validateCodeService;

    public ValidateVerificationCode(ILogger<ValidateVerificationCode> logger, ValidateVerificationCodeService validateCodeService)
    {
        _logger = logger;
        _validateCodeService = validateCodeService;
    }

    [Function("ValidateVerificationCode")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "validate")] HttpRequest req)
    {
        try
        {
            var validateRequest = await _validateCodeService.UnpackValidateRequestAsync(req);
            if (validateRequest != null)
            {
                var validateResult = await _validateCodeService.ValidateCodeAsync(validateRequest);
                if (validateResult)
                {
                    return new OkResult();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: ValidateVerificationCode.Run :: {ex.Message}");
        }

        return new UnauthorizedResult();
    }
}
