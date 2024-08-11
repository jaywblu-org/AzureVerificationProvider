using Azure.Messaging.ServiceBus;
using AzureVerificationProvider.Services;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Grpc.Core.Metadata;

namespace AzureVerificationProvider.Functions;

public class GenerateVerificationCodeWithHttp
{
    private readonly ILogger<GenerateVerificationCodeWithHttp> _logger;
    private readonly VerificationCodeService _verificationCodeService;
    private readonly ServiceBusClient _serviceBusClient;

    public GenerateVerificationCodeWithHttp(ILogger<GenerateVerificationCodeWithHttp> logger, VerificationCodeService verificationCodeService, ServiceBusClient client)
    {
        _logger = logger;
        _verificationCodeService = verificationCodeService;
        _serviceBusClient = client;
    }

    [Function("GenerateVerificationCodeWithHttp")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {

        var sender = _serviceBusClient.CreateSender("email_request");

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
                                var message = new ServiceBusMessage(payload)
                                {
                                    ContentType = "application/json"
                                };

                                await sender.SendMessageAsync(message);

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
        finally
        {
            await sender.DisposeAsync();
        }

        return new BadRequestResult();
    }
}
