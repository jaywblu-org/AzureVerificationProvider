using AzureVerificationProvider.Data.Contexts;
using AzureVerificationProvider.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureVerificationProvider.Services;

public class VerificationCleanerService(ILogger<VerificationCleanerService> logger, DataContext context)
{
    private readonly ILogger<VerificationCleanerService> _logger = logger;
    private readonly DataContext _context = context;

    public async Task RemoveExpiredRecordsAsync()
    {
        try
        {
            var expired = await _context.VerificationRequests.Where(x => x.ExpiryDate <= DateTime.Now).ToListAsync();
            _context.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: VerificationCleaner.RemoveExpiredRecordsAsync :: {ex.Message}");
        }
    }
}
