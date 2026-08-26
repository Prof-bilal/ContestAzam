using EventSphere.Api.Common;
using EventSphere.Api.DTOs;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Api.Controllers;

[ApiController]
[Route("api")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost("payment/create-checkout")]
    [Authorize(Roles = $"{AppRoles.Visitor},{AppRoles.Participant}")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutRequest request)
    {
        var userIdValue = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        try
        {
            var origin = $"{Request.Scheme}://{Request.Host}";
            var session = await _paymentService.CreateCheckoutSessionAsync(request.EventId, userId, origin);
            return Ok(ApiResponse<PaymentSessionDto>.Ok(session));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("webhook/stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        var success = await _paymentService.HandleWebhookAsync(json, signature);
        return success ? Ok() : BadRequest();
    }

    [HttpGet("payment/publishable-key")]
    [AllowAnonymous]
    public IActionResult GetPublishableKey()
    {
        var key = _paymentService.GetPublishableKey();
        return Ok(ApiResponse<object>.Ok(new { key }));
    }

    [HttpGet("payment/status/{eventId:int}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentStatus(int eventId)
    {
        var userIdValue = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var status = await _paymentService.GetPaymentStatusAsync(eventId, userId);
        return Ok(ApiResponse<PaymentDto?>.Ok(status));
    }
}
