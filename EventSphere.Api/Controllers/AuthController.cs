using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using EventSphere.Api.Auth;
using EventSphere.Api.Common;
using EventSphere.Api.Common.Options;
using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace EventSphere.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshService;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _db;
    private readonly IAuthenticationSchemeProvider _schemes;
    private readonly RefreshTokenOptions _refreshOptions;
    private readonly FrontendOptions _frontend;
    private readonly ILogger<AuthController> _logger;
    private readonly IdentityOptions _identityOptions;
    private readonly IDataProtector _oauthProtector;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IRefreshTokenService refreshService,
        IEmailService emailService,
        AppDbContext db,
        IAuthenticationSchemeProvider schemes,
        IOptions<RefreshTokenOptions> refreshOptions,
        IOptions<FrontendOptions> frontend,
        IOptions<IdentityOptions> identityOptions,
        IDataProtectionProvider dataProtection,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _refreshService = refreshService;
        _emailService = emailService;
        _db = db;
        _schemes = schemes;
        _refreshOptions = refreshOptions.Value;
        _frontend = frontend.Value;
        _logger = logger;
        _identityOptions = identityOptions.Value;
        _oauthProtector = dataProtection.CreateProtector("EventSphere.OAuth.Pending");
    }

    // ---------------------------------------------------------------- Register
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var email = request.Email.Trim();

        // Validate AccountType — only "Visitor" and "Organizer" are accepted.
        // Participant and Admin must never be assigned through registration.
        var accountType = request.AccountType?.Trim();
        if (!string.Equals(accountType, "Visitor", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(accountType, "Organizer", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse.Fail("Invalid account type.", "accountType",
                "Account type must be 'Visitor' or 'Organizer'."));
        }

        // When Organizer is selected, validate required organizer fields.
        var isOrganizerRequest = string.Equals(accountType, "Organizer", StringComparison.OrdinalIgnoreCase);
        if (isOrganizerRequest)
        {
            if (string.IsNullOrWhiteSpace(request.OrganizationName))
                return BadRequest(ApiResponse.Fail("Organization name is required for organizer accounts.", "organizationName",
                    "Organization name is required."));
            if (string.IsNullOrWhiteSpace(request.OrganizationReason))
                return BadRequest(ApiResponse.Fail("Reason is required for organizer accounts.", "organizationReason",
                    "Please explain why you want to organize events."));
        }

        if (await _userManager.FindByEmailAsync(email) is not null)
            return Conflict(ApiResponse.Fail("An account with this email already exists.", "email",
                "An account with this email already exists."));

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            Role = UserRole.Visitor
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return BadRequest(ApiResponse.Fail("Registration failed.", MapIdentityErrors(created)));

        // Default role is always Visitor, assigned server-side.
        await _userManager.AddToRoleAsync(user, AppRoles.Default);

        _db.UserDetails.Add(new UserDetails { UserId = user.Id, FullName = request.Name.Trim() });

        // If Organizer was requested, create a pending OrganizerRequest.
        if (isOrganizerRequest)
        {
            _db.OrganizerRequests.Add(new OrganizerRequest
            {
                UserId = user.Id,
                OrganizationName = request.OrganizationName!.Trim(),
                Reason = request.OrganizationReason!.Trim(),
                Experience = request.OrganizationExperience?.Trim(),
                Status = OrganizerRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        // Generate email verification token and send verification email.
        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var frontendUrl = _frontend.AllowedOrigins.FirstOrDefault() ?? "http://localhost:5173";
            var encodedToken = UrlEncoder.Default.Encode(token);
            var encodedEmail = UrlEncoder.Default.Encode(email);
            var verificationUrl = $"{frontendUrl.TrimEnd('/')}/verify-email?token={encodedToken}&email={encodedEmail}";

            await _emailService.SendEmailVerificationAsync(email, request.Name.Trim(), verificationUrl);
        }
        catch (Exception ex)
        {
            // Do not let email failure prevent account creation.
            _logger.LogWarning(ex, "Failed to send verification email to {Email}.", email);
        }

        var response = await IssueSessionAsync(user, request.Name.Trim());
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(response, "Account created. Check your email to verify your account."));
    }

    // ------------------------------------------------------------------- Login
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        // Generic message regardless of whether the email exists (anti-enumeration).
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse.Fail("Invalid email or password."));

        if (_identityOptions.SignIn.RequireConfirmedAccount && !await _userManager.IsEmailConfirmedAsync(user))
            return Unauthorized(ApiResponse.Fail("Please verify your email before signing in.", "emailVerificationRequired", "Email verification required."));

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var remaining = lockoutEnd.HasValue
                ? (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalSeconds)
                : 900;
            return StatusCode(StatusCodes.Status423Locked,
                ApiResponse.Fail($"Account temporarily locked due to repeated failed attempts. Try again in {remaining} seconds.", "retryAfter", remaining.ToString()));
        }

        if (!result.Succeeded)
            return Unauthorized(ApiResponse.Fail("Invalid email or password."));

        var fullName = await GetFullNameAsync(user.Id) ?? user.Email!;
        var response = await IssueSessionAsync(user, fullName);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Welcome back."));
    }

    // ----------------------------------------------------------------- Refresh
    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh()
    {
        var raw = Request.Cookies[_refreshOptions.CookieName];
        if (string.IsNullOrEmpty(raw))
            return Unauthorized(ApiResponse.Fail("No active session."));

        var rotation = await _refreshService.RotateAsync(raw, ClientIp());

        if (rotation.Outcome != RefreshOutcome.Success)
        {
            ClearRefreshCookie();
            var message = rotation.Outcome == RefreshOutcome.Reuse
                ? "Your session was invalidated for security reasons. Please sign in again."
                : "Your session has expired. Please sign in again.";
            return Unauthorized(ApiResponse.Fail(message));
        }

        var user = await _userManager.FindByIdAsync(rotation.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            ClearRefreshCookie();
            return Unauthorized(ApiResponse.Fail("Your session has expired. Please sign in again."));
        }

        SetRefreshCookie(rotation.NewRawToken!, rotation.NewExpiresAtUtc!.Value);

        var fullName = await GetFullNameAsync(user.Id) ?? user.Email!;
        var roles = await _userManager.GetRolesAsync(user);
        var displayRoles = FilterDisplayRoles(roles);
        var (access, exp) = _tokenService.GenerateAccessToken(user, roles, fullName);

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = access,
            AccessTokenExpiresAtUtc = exp,
            User = ToDto(user, displayRoles, fullName)
        }, "Session refreshed."));
    }

    // ------------------------------------------------------------------ Logout
    // No [Authorize]: logout must work even with an expired access token, using the cookie.
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var raw = Request.Cookies[_refreshOptions.CookieName];
        if (!string.IsNullOrEmpty(raw))
            await _refreshService.RevokeAsync(raw);

        ClearRefreshCookie();
        return Ok(ApiResponse.Ok("Signed out."));
    }

    // --------------------------------------------------------------------- Me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var idValue = User.FindFirstValue("sub");
        if (!int.TryParse(idValue, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var fullName = await GetFullNameAsync(user.Id) ?? user.Email!;
        var roles = await _userManager.GetRolesAsync(user);
        // For display: filter to highest-privilege role only.
        // Visitor is removed if a higher role exists.
        var displayRoles = FilterDisplayRoles(roles);
        return Ok(ApiResponse<UserDto>.Ok(ToDto(user, displayRoles, fullName)));
    }

    // --------------------------------------------------------- Verify Email
    [HttpPost("verify-email")]
    [EnableRateLimiting("email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return BadRequest(ApiResponse.Fail("Invalid verification request."));

        if (await _userManager.IsEmailConfirmedAsync(user))
            return Ok(ApiResponse.Ok("Email is already verified."));

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
            return BadRequest(ApiResponse.Fail("Invalid or expired verification token."));

        return Ok(ApiResponse.Ok("Email verified successfully."));
    }

    // --------------------------------------------------------- Resend Verification
    [HttpPost("resend-verification")]
    [EnableRateLimiting("email")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        // Always return the same response to prevent email enumeration.
        if (user is null)
            return Ok(ApiResponse.Ok("If an account exists for this email, a verification link has been sent."));

        if (await _userManager.IsEmailConfirmedAsync(user))
            return Ok(ApiResponse.Ok("If an account exists for this email, a verification link has been sent."));

        if (!user.IsActive)
            return Ok(ApiResponse.Ok("If an account exists for this email, a verification link has been sent."));

        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var frontendUrl = _frontend.AllowedOrigins.FirstOrDefault() ?? "http://localhost:5173";
            var encodedToken = UrlEncoder.Default.Encode(token);
            var encodedEmail = UrlEncoder.Default.Encode(user.Email!);
            var verificationUrl = $"{frontendUrl.TrimEnd('/')}/verify-email?token={encodedToken}&email={encodedEmail}";
            var fullName = await GetFullNameAsync(user.Id) ?? user.Email!;

            await _emailService.SendEmailVerificationAsync(user.Email!, fullName, verificationUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send verification email to {Email}.", user.Email);
        }

        return Ok(ApiResponse.Ok("If an account exists for this email, a verification link has been sent."));
    }

    // --------------------------------------------------------- Forgot Password
    [HttpPost("forgot-password")]
    [EnableRateLimiting("email")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        // Always return the same response — never reveal whether the email exists.
        var safeResponse = ApiResponse.Ok("If an account exists for this email, we sent a password reset link.");

        if (user is null || !user.IsActive)
            return Ok(safeResponse);

        try
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontendUrl = _frontend.AllowedOrigins.FirstOrDefault() ?? "http://localhost:5173";
            var encodedToken = UrlEncoder.Default.Encode(token);
            var encodedEmail = UrlEncoder.Default.Encode(user.Email!);
            var resetUrl = $"{frontendUrl.TrimEnd('/')}/reset-password?token={encodedToken}&email={encodedEmail}";
            var fullName = await GetFullNameAsync(user.Id) ?? user.Email!;

            await _emailService.SendPasswordResetAsync(user.Email!, fullName, resetUrl);
        }
        catch (Exception ex)
        {
            // Log the failure server-side but still return the generic response.
            _logger.LogWarning(ex, "Failed to send password reset email to {Email}.", request.Email);
        }

        return Ok(safeResponse);
    }

    // --------------------------------------------------------- Reset Password
    [HttpPost("reset-password")]
    [EnableRateLimiting("email")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return BadRequest(ApiResponse.Fail("Invalid reset request."));

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(ApiResponse.Fail("Invalid or expired reset token.", MapIdentityErrors(result)));

        // Revoke all existing refresh tokens for this user to force re-authentication.
        await _refreshService.RevokeAllForUserAsync(user.Id);

        return Ok(ApiResponse.Ok("Password reset successfully."));
    }

    // ---------------------------------------------------- External OAuth: init
    [HttpGet("external/{provider}")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ExternalLogin(string provider)
    {
        var scheme = NormalizeProvider(provider);
        if (scheme is null || await _schemes.GetSchemeAsync(scheme) is null)
            return RedirectToFrontendError("provider_unavailable");

        var callbackUrl = Url.Action(nameof(ExternalCallback), "Auth", values: null, protocol: Request.Scheme);
        var props = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items = { ["LoginProvider"] = scheme }
        };
        return Challenge(props, scheme);
    }

    // ------------------------------------------------ External OAuth: callback
    [HttpGet("external/callback")]
    public async Task<IActionResult> ExternalCallback()
    {
        var result = await HttpContext.AuthenticateAsync(ExternalAuth.CookieScheme);
        if (!result.Succeeded || result.Principal is null)
            return RedirectToFrontendError("oauth_failed");

        var provider = result.Properties?.Items.TryGetValue("LoginProvider", out var p) == true ? p : null;
        var providerKey = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var emailVerified = string.Equals(
            result.Principal.FindFirstValue(ExternalAuth.EmailVerifiedClaim), "true", StringComparison.OrdinalIgnoreCase);
        var name = result.Principal.FindFirstValue("name")
                   ?? result.Principal.FindFirstValue("urn:github:login")
                   ?? email?.Split('@')[0]
                   ?? "User";

        // Always clear the temporary external cookie once consumed.
        await HttpContext.SignOutAsync(ExternalAuth.CookieScheme);

        if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(providerKey))
            return RedirectToFrontendError("oauth_failed");

        // 1) Already-linked external login -> sign in.
        var user = await _userManager.FindByLoginAsync(provider, providerKey);

        if (user is not null)
        {
            // Found via login link. If inactive (deleted account), reactivate.
            if (!user.IsActive)
            {
                user.Email = email;
                user.UserName = email;
                user.EmailConfirmed = true;
                user.IsActive = true;
                user.Role = UserRole.Visitor;
                await _userManager.UpdateAsync(user);
                await _userManager.UpdateNormalizedEmailAsync(user);

                var oldRoles = await _userManager.GetRolesAsync(user);
                if (oldRoles.Count > 0)
                    await _userManager.RemoveFromRolesAsync(user, oldRoles);
                await _userManager.AddToRoleAsync(user, AppRoles.Default);
            }

            var (rawRefresh, expires) = await _refreshService.IssueAsync(user.Id, ClientIp());
            SetRefreshCookie(rawRefresh, expires);
            return RedirectToFrontendSuccess();
        }

        // 2) No linked login. Check if an account with this email already exists.
        if (string.IsNullOrEmpty(email))
            return RedirectToFrontendError("email_required");

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            // 3) Existing account with this email.
            if (!existing.IsActive)
            {
                // Deleted/inactive account — allow re-registration via OAuth.
                // Reactivate the account.
                existing.Email = email;
                existing.UserName = email;
                existing.EmailConfirmed = true;
                existing.IsActive = true;
                existing.Role = UserRole.Visitor;
                await _userManager.UpdateAsync(existing);
                await _userManager.UpdateNormalizedEmailAsync(existing);

                // Remove old roles and assign fresh Visitor role.
                var oldRoles = await _userManager.GetRolesAsync(existing);
                if (oldRoles.Count > 0)
                    await _userManager.RemoveFromRolesAsync(existing, oldRoles);
                await _userManager.AddToRoleAsync(existing, AppRoles.Default);

                // Link the external login.
                var deletedLogins = await _userManager.GetLoginsAsync(existing);
                if (!deletedLogins.Any(l => l.LoginProvider == provider && l.ProviderKey == providerKey))
                    await _userManager.AddLoginAsync(existing, new UserLoginInfo(provider, providerKey, provider));

                // Issue session.
                var (refreshNew, expiresNew) = await _refreshService.IssueAsync(existing.Id, ClientIp());
                SetRefreshCookie(refreshNew, expiresNew);
                return RedirectToFrontendSuccess();
            }

            // Active account — link the provider and sign in.
            // Preserve existing roles — OAuth must never upgrade or downgrade.

            // Link the external login if not already linked.
            var existingLogins = await _userManager.GetLoginsAsync(existing);
            if (!existingLogins.Any(l => l.LoginProvider == provider && l.ProviderKey == providerKey))
            {
                var linkResult = await _userManager.AddLoginAsync(existing, new UserLoginInfo(provider, providerKey, provider));
                if (!linkResult.Succeeded)
                    return RedirectToFrontendError("oauth_failed");
            }

            // Issue session — preserve all existing roles.
            var (rawRefresh, expires) = await _refreshService.IssueAsync(existing.Id, ClientIp());
            SetRefreshCookie(rawRefresh, expires);
            return RedirectToFrontendSuccess();
        }

        // 4) Brand-new user. Only create from a provider-verified email.
        if (!emailVerified)
            return RedirectToFrontendError("email_unverified");

        // Instead of creating the user immediately, redirect to the frontend
        // OAuth completion page with a secure pending token. The frontend will
        // let the user choose Visitor or Organizer, then POST to complete.
        var pendingData = JsonSerializer.Serialize(new PendingOAuthInfo
        {
            Provider = provider,
            ProviderKey = providerKey,
            Email = email,
            Name = name
        });
        var protectedToken = _oauthProtector.Protect(pendingData);
        var encodedToken = UrlEncoder.Default.Encode(protectedToken);

        var baseUrl = _frontend.AllowedOrigins.FirstOrDefault();
        if (string.IsNullOrEmpty(baseUrl))
            return Ok(ApiResponse.Ok("OAuth pending. Complete registration."));

        return Redirect($"{baseUrl.TrimEnd('/')}/oauth/complete?pending={encodedToken}");
    }

    /// <summary>
    /// Completes OAuth registration for a new user. The pending token must be
    /// obtained from the OAuth callback. The user chooses Visitor or Organizer.
    /// </summary>
    [HttpPost("external/complete")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CompleteOAuthRegistration([FromBody] CompleteOAuthRegistrationRequest request)
    {
        if (string.IsNullOrEmpty(request.PendingToken))
            return BadRequest(ApiResponse.Fail("Invalid registration request."));

        // Validate account type.
        var accountType = request.AccountType?.Trim();
        if (!string.Equals(accountType, "Visitor", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(accountType, "Organizer", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse.Fail("Invalid account type.", "accountType",
                "Account type must be 'Visitor' or 'Organizer'."));
        }

        PendingOAuthInfo pendingInfo;
        try
        {
            var decoded = Uri.UnescapeDataString(request.PendingToken);
            var json = _oauthProtector.Unprotect(decoded);
            pendingInfo = JsonSerializer.Deserialize<PendingOAuthInfo>(json)!;
        }
        catch
        {
            return BadRequest(ApiResponse.Fail("Invalid or expired registration token. Please try signing in again."));
        }

        // Verify the email hasn't been used in the meantime.
        if (await _userManager.FindByEmailAsync(pendingInfo.Email) is not null)
        {
            // Account was created between the callback and now — just log in.
            var existingUser = await _userManager.FindByEmailAsync(pendingInfo.Email);
            if (existingUser is null || !existingUser.IsActive)
                return RedirectToFrontendError("account_disabled");

            var (rawRefresh, expires) = await _refreshService.IssueAsync(existingUser.Id, ClientIp());
            SetRefreshCookie(rawRefresh, expires);

            var fullName = await GetFullNameAsync(existingUser.Id) ?? existingUser.Email!;
            var roles = await _userManager.GetRolesAsync(existingUser);
            var displayRoles = FilterDisplayRoles(roles);
            var (access, exp) = _tokenService.GenerateAccessToken(existingUser, roles, fullName);

            return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
            {
                AccessToken = access,
                AccessTokenExpiresAtUtc = exp,
                User = ToDto(existingUser, displayRoles, fullName)
            }, "Account already exists. Signed in."));
        }

        // Validate organizer fields if applicable.
        var isOrganizerRequest = string.Equals(accountType, "Organizer", StringComparison.OrdinalIgnoreCase);
        if (isOrganizerRequest)
        {
            if (string.IsNullOrWhiteSpace(request.OrganizationName))
                return BadRequest(ApiResponse.Fail("Organization name is required for organizer accounts.", "organizationName",
                    "Organization name is required."));
            if (string.IsNullOrWhiteSpace(request.OrganizationReason))
                return BadRequest(ApiResponse.Fail("Reason is required for organizer accounts.", "organizationReason",
                    "Please explain why you want to organize events."));
        }

        // Create the user.
        var user = new AppUser
        {
            UserName = pendingInfo.Email,
            Email = pendingInfo.Email,
            EmailConfirmed = true,
            Role = UserRole.Visitor
        };

        var created = await _userManager.CreateAsync(user);
        if (!created.Succeeded)
            return BadRequest(ApiResponse.Fail("Registration failed.", MapIdentityErrors(created)));

        await _userManager.AddToRoleAsync(user, AppRoles.Default);
        await _userManager.AddLoginAsync(user, new UserLoginInfo(pendingInfo.Provider, pendingInfo.ProviderKey, pendingInfo.Provider));

        _db.UserDetails.Add(new UserDetails { UserId = user.Id, FullName = pendingInfo.Name });

        if (isOrganizerRequest)
        {
            _db.OrganizerRequests.Add(new OrganizerRequest
            {
                UserId = user.Id,
                OrganizationName = request.OrganizationName!.Trim(),
                Reason = request.OrganizationReason!.Trim(),
                Experience = request.OrganizationExperience?.Trim(),
                Status = OrganizerRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        // Issue session.
        var response = await IssueSessionAsync(user, pendingInfo.Name);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(response, isOrganizerRequest
                ? "Account created. Your organizer application is pending admin review."
                : "Account created. Welcome to EventSphere!"));
    }

    // ------------------------------------------------------------- Organizer Requests (user-facing)

    /// <summary>Submit a new organizer request (for existing users who did not register as organizer).</summary>
    [HttpPost("organizer-requests")]
    [Authorize]
    public async Task<IActionResult> CreateOrganizerRequest([FromBody] CreateOrganizerRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        if (!await _userManager.IsEmailConfirmedAsync(user))
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail("Please verify your email before submitting an organizer request."));

        if (await _userManager.IsInRoleAsync(user, AppRoles.Organizer))
            return Conflict(ApiResponse.Fail("You are already an organizer."));

        // Block duplicate pending requests.
        var hasPending = await _db.OrganizerRequests
            .AnyAsync(r => r.UserId == userId.Value && r.Status == OrganizerRequestStatus.Pending);
        if (hasPending)
            return Conflict(ApiResponse.Fail("You already have a pending organizer request."));

        _db.OrganizerRequests.Add(new OrganizerRequest
        {
            UserId = userId.Value,
            OrganizationName = request.OrganizationName.Trim(),
            Reason = request.Reason.Trim(),
            Experience = request.Experience?.Trim(),
            Status = OrganizerRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse.Ok("Organizer request submitted. An admin will review your application."));
    }

    /// <summary>Get the current user's organizer request status.</summary>
    [HttpGet("organizer-requests/me")]
    [Authorize]
    public async Task<IActionResult> GetMyOrganizerRequest()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var request = await _db.OrganizerRequests
            .Where(r => r.UserId == userId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new OrganizerRequestDto
            {
                Id = r.Id,
                OrganizationName = r.OrganizationName,
                Reason = r.Reason,
                Experience = r.Experience,
                Status = r.Status.ToString(),
                RejectionReason = r.RejectionReason,
                ReviewedAt = r.ReviewedAt,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (request is null)
            return NotFound(ApiResponse.Fail("No organizer request found."));

        return Ok(ApiResponse<OrganizerRequestDto>.Ok(request));
    }

    // ------------------------------------------------------------- Helpers
    private int? GetUserId()
    {
        var idValue = User.FindFirstValue("sub");
        return int.TryParse(idValue, out var userId) ? userId : null;
    }

    /// <summary>
    /// Filters roles for display purposes. Removes Visitor when a higher-privilege
    /// role exists, so the UI only shows the most significant role.
    /// </summary>
    private static IList<string> FilterDisplayRoles(IList<string> roles)
    {
        if (roles.Count <= 1) return roles;
        var filtered = roles.Where(r => r != AppRoles.Visitor).ToList();
        return filtered.Count > 0 ? filtered : roles;
    }

    private async Task<AuthResponse> IssueSessionAsync(AppUser user, string fullName)
    {
        var (rawRefresh, expires) = await _refreshService.IssueAsync(user.Id, ClientIp());
        SetRefreshCookie(rawRefresh, expires);

        var roles = await _userManager.GetRolesAsync(user);
        var displayRoles = FilterDisplayRoles(roles);
        var (access, exp) = _tokenService.GenerateAccessToken(user, roles, fullName);

        return new AuthResponse
        {
            AccessToken = access,
            AccessTokenExpiresAtUtc = exp,
            User = ToDto(user, displayRoles, fullName)
        };
    }

    private UserDto ToDto(AppUser user, IList<string> roles, string fullName) => new()
    {
        Id = user.Id,
        Name = fullName,
        Email = user.Email ?? string.Empty,
        Roles = roles.ToArray(),
        CreatedAt = user.CreatedAt
    };

    private Task<string?> GetFullNameAsync(int userId) =>
        _db.UserDetails.Where(d => d.UserId == userId).Select(d => d.FullName).FirstOrDefaultAsync();

    private static IDictionary<string, string[]> MapIdentityErrors(IdentityResult result)
    {
        // Map Identity error codes to client fields without leaking internals.
        var password = new List<string>();
        var email = new List<string>();
        var general = new List<string>();

        foreach (var e in result.Errors)
        {
            if (e.Code.Contains("Password", StringComparison.OrdinalIgnoreCase)) password.Add(e.Description);
            else if (e.Code.Contains("Email", StringComparison.OrdinalIgnoreCase)
                     || e.Code.Contains("UserName", StringComparison.OrdinalIgnoreCase)) email.Add(e.Description);
            else general.Add(e.Description);
        }

        var dict = new Dictionary<string, string[]>();
        if (password.Count > 0) dict["password"] = password.ToArray();
        if (email.Count > 0) dict["email"] = email.ToArray();
        if (general.Count > 0) dict["general"] = general.ToArray();
        return dict;
    }

    private static string? NormalizeProvider(string provider) => provider?.ToLowerInvariant() switch
    {
        "google" => ExternalAuth.Google,
        "github" => ExternalAuth.GitHub,
        _ => null
    };

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private void SetRefreshCookie(string token, DateTime expiresUtc)
    {
        var sameSite = ParseSameSite(_refreshOptions.CookieSameSite);
        Response.Cookies.Append(_refreshOptions.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = sameSite == SameSiteMode.None || Request.IsHttps,
            SameSite = sameSite,
            Expires = new DateTimeOffset(expiresUtc, TimeSpan.Zero),
            Path = "/api/auth",
            IsEssential = true
        });
    }

    private void ClearRefreshCookie()
    {
        var sameSite = ParseSameSite(_refreshOptions.CookieSameSite);
        Response.Cookies.Delete(_refreshOptions.CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = sameSite == SameSiteMode.None || Request.IsHttps,
            SameSite = sameSite,
            Path = "/api/auth"
        });
    }

    private static SameSiteMode ParseSameSite(string value) => value?.ToLowerInvariant() switch
    {
        "none" => SameSiteMode.None,
        "strict" => SameSiteMode.Strict,
        _ => SameSiteMode.Lax
    };

    private IActionResult RedirectToFrontendSuccess()
    {
        var baseUrl = _frontend.AllowedOrigins.FirstOrDefault();
        if (string.IsNullOrEmpty(baseUrl))
            return Ok(ApiResponse.Ok("Signed in. Refresh your session to continue."));
        return Redirect($"{baseUrl.TrimEnd('/')}{_frontend.PostLoginRedirectPath}");
    }

    private IActionResult RedirectToFrontendError(string code)
    {
        var baseUrl = _frontend.AllowedOrigins.FirstOrDefault();
        if (string.IsNullOrEmpty(baseUrl))
            return Unauthorized(ApiResponse.Fail("External sign-in failed."));
        return Redirect($"{baseUrl.TrimEnd('/')}{_frontend.PostLoginErrorPath}?error={Uri.EscapeDataString(code)}");
    }
}

/// <summary>Data packed into the pending OAuth registration token.</summary>
internal class PendingOAuthInfo
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
