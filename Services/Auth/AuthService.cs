using fifa_backend.Data;
using fifa_backend.DTOs.Auth;
using fifa_backend.Exceptions;
using fifa_backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using fifa_backend.Services.Audit;
using fifa_backend.Services.Email;

namespace fifa_backend.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;

    public AuthService(
        AppDbContext context,
        IJwtService jwtService,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService)
    {
        _context = context;
        _jwtService = jwtService;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
    }

    public async Task<bool> SendOtpAsync(SendOtpRequest request)
    {
        var emailNormalized = request.Email.ToLowerInvariant().Trim();

        // Check if user exists (including soft-deleted)
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == emailNormalized);

        if (user != null && (!user.IsActive || user.IsDeleted))
        {
            throw new BadRequestException("Your account is deactivated. Please contact support.");
        }

        var otp = Random.Shared.Next(100000, 999999).ToString();

        var otpEntry = new OtpVerification
        {
            Email = emailNormalized,
            OtpCode = otp,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        _context.OtpVerifications.Add(otpEntry);
        await _context.SaveChangesAsync();

        // Send actual transactional email via Brevo
        await _emailService.SendOtpEmailAsync(emailNormalized, otp);

        // Logging the OTP to console for debugging/development ease.
        Console.WriteLine($"[DEVELOPMENT ONLY] OTP FOR {emailNormalized} = {otp}");

        return true;
    }

    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var emailNormalized = request.Email.ToLowerInvariant().Trim();

        var otp = await _context.OtpVerifications
            .Where(x =>
                x.Email == emailNormalized &&
                x.OtpCode == request.Otp &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            await _auditLogService.LogAsync(
                null,
                "LOGIN_FAILED",
                "User",
                $"OTP verification failed for email: {emailNormalized}",
                _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
            );
            throw new BadRequestException("Invalid or expired OTP code.");
        }

        otp.IsUsed = true;

        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == emailNormalized);

        if (user != null)
        {
            if (!user.IsActive || user.IsDeleted)
            {
                await _auditLogService.LogAsync(
                    user.Id,
                    "LOGIN_FAILED",
                    "User",
                    $"Deactivated user attempted to login: {user.Email}",
                    _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                );
                throw new BadRequestException("Your account is deactivated. Please contact support.");
            }
            user.EmailVerified = true;
        }
        else
        {
            user = new User
            {
                Email = emailNormalized,
                UserName = emailNormalized.Split('@')[0],
                EmailVerified = true,
                IsActive = true
            };

            _context.Users.Add(user);
        }

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            user.Id,
            "LOGIN",
            "User",
            $"User OTP verification successful. Logged in: {user.Email}",
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        );

        var token = _jwtService.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            UserName = user.UserName,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var emailOrUsernameNormalized = request.EmailOrUsername.Trim();

        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == emailOrUsernameNormalized || x.UserName == emailOrUsernameNormalized);

        if (user == null || !user.IsActive || user.IsDeleted || string.IsNullOrEmpty(user.PasswordHash))
        {
            await _auditLogService.LogAsync(
                null,
                "LOGIN_FAILED",
                "User",
                $"Failed login attempt (invalid credentials or no password) for username/email: {emailOrUsernameNormalized}",
                _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
            );
            throw new BadRequestException("Invalid email/username or password.");
        }

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        {
            await _auditLogService.LogAsync(
                user.Id,
                "LOGIN_FAILED",
                "User",
                $"Failed login attempt (incorrect password) for user: {user.Email}",
                _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
            );
            throw new BadRequestException("Invalid email/username or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            user.Id,
            "LOGIN",
            "User",
            $"User logged in successfully via password: {user.Email}",
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        );

        var token = _jwtService.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            UserName = user.UserName,
            Role = user.Role.ToString()
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var emailNormalized = request.Email.ToLowerInvariant().Trim();
        var usernameNormalized = request.UserName.Trim();

        // Check if username or email already exists
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == emailNormalized || u.UserName == usernameNormalized);

        if (existingUser != null)
        {
            if (existingUser.IsDeleted || !existingUser.IsActive)
            {
                throw new BadRequestException("Your account is deactivated. Please contact support.");
            }
            throw new ConflictException("A user with this email or username already exists.");
        }

        // Create inactive, unverified user
        var user = new User
        {
            Email = emailNormalized,
            UserName = usernameNormalized,
            EmailVerified = false,
            IsActive = true
        };

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send verification OTP
        var otp = Random.Shared.Next(100000, 999999).ToString();
        var otpEntry = new OtpVerification
        {
            Email = emailNormalized,
            OtpCode = otp,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        _context.OtpVerifications.Add(otpEntry);
        await _context.SaveChangesAsync();

        await _emailService.SendOtpEmailAsync(emailNormalized, otp);

        Console.WriteLine($"[DEVELOPMENT ONLY] REGISTRATION OTP FOR {emailNormalized} = {otp}");

        await _auditLogService.LogAsync(
            user.Id,
            "REGISTER",
            "User",
            $"User registered: {user.Email}. Sent verification OTP.",
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        );

        return true;
    }
}