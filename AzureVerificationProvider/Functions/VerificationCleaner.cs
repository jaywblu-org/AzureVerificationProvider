using AzureVerificationProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureVerificationProvider.Functions;

public class VerificationCleaner(ILogger<VerificationCleaner> logger, VerificationCleanerService verificationCleanerService)
{
    private readonly ILogger<VerificationCleaner> _logger = logger;
    private readonly VerificationCleanerService _verificationCleanerService = verificationCleanerService;

    [Function("VerificationCleaner")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        try
        {
            await _verificationCleanerService.RemoveExpiredRecordsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: VerificationCleaner.Run :: {ex.Message}");
        }
    }
}
