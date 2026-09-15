using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalExpenseManager.Api.Common;
using PersonalExpenseManager.Application.Auth.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail("Email is already registered.", nameof(request.Email)));
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new ApiError("", e.Description));
            return UnprocessableEntity(ApiResponse<object>.Fail(errors));
        }

        logger.LogInformation("New user registered: {UserId}", user.Id);

        await signInManager.SignInAsync(user, isPersistent: true);

        return Ok(ApiResponse<UserProfileDto>.Ok(ToProfile(user)));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid email or password."));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse<object>.Fail("This account has been disabled."));
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: true, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("Account locked out after repeated failed attempts: {UserId}", user.Id);
            return Unauthorized(ApiResponse<object>.Fail("Too many failed attempts. Try again later."));
        }

        if (!result.Succeeded)
        {
            logger.LogInformation("Failed login attempt for {UserId}", user.Id);
            return Unauthorized(ApiResponse<object>.Fail("Invalid email or password."));
        }

        logger.LogInformation("User logged in: {UserId}", user.Id);

        return Ok(ApiResponse<UserProfileDto>.Ok(ToProfile(user)));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.GetUserId();
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out: {UserId}", userId);

        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Not authenticated."));
        }

        return Ok(ApiResponse<UserProfileDto>.Ok(ToProfile(user)));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is not null && user.IsActive)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            await emailSender.SendAsync(
                user.Email!,
                "Reset your password",
                $"<p>Click <a href=\"{resetLink}\">here</a> to reset your password. This link will expire soon.</p>",
                cancellationToken);

            logger.LogInformation("Password reset requested for {UserId}", user.Id);
        }

        return Ok(ApiResponse<object>.Ok(new { message = "If an account exists for this email, a password reset link has been sent." }));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid or expired reset token."));
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid or expired reset token."));
        }

        await userManager.UpdateSecurityStampAsync(user);
        logger.LogInformation("Password reset completed for {UserId}", user.Id);

        return Ok(ApiResponse<object>.Ok(new { message = "Password has been reset. You can now sign in." }));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Not authenticated."));
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new ApiError("", e.Description));
            return UnprocessableEntity(ApiResponse<object>.Fail(errors));
        }

        await signInManager.RefreshSignInAsync(user);
        logger.LogInformation("Password changed for {UserId}", user.Id);

        return Ok(ApiResponse<object>.Ok(new { message = "Password changed successfully." }));
    }

    private static UserProfileDto ToProfile(ApplicationUser user) => new(user.Id, user.FirstName, user.LastName, user.Email!);
}
