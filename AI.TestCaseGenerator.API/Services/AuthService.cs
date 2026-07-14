using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.Auth;
using AI.TestCaseGenerator.API.DTOs.User;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace AI.TestCaseGenerator.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
    var existingUser = await _context.Users
        .FirstOrDefaultAsync(x =>
    x.Email.ToLower() == request.Email.ToLower());

    if (existingUser != null)
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = "Email already exists."
        };
    }

    User user = new()
    {
        FullName = request.FullName,
        Email = request.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
    };

    _context.Users.Add(user);

    await _context.SaveChangesAsync();

    string jwtToken = GenerateJwtToken(user);

    string refreshToken = GenerateRefreshToken();

    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

    await _context.SaveChangesAsync();

    return new AuthResponseDto
    {
        Success = true,
        Message = "Registration successful.",
        Token = jwtToken,
        RefreshToken = refreshToken,
        Expiration = DateTime.UtcNow.AddHours(2),
        User = _mapper.Map<UserProfileDto>(user)
    };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(x =>
    x.Email.ToLower() == request.Email.ToLower());

    if (user == null)
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = "Invalid email or password."
        };
    }

    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
        request.Password,
        user.PasswordHash);

    if (!isPasswordValid)
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = "Invalid email or password."
        };
    }

    string jwtToken = GenerateJwtToken(user);

    string refreshToken = GenerateRefreshToken();

    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

    await _context.SaveChangesAsync();

    return new AuthResponseDto
    {
        Success = true,
        Message = "Login successful.",
        Token = jwtToken,
        RefreshToken = refreshToken,
        Expiration = DateTime.UtcNow.AddHours(2),
        User = _mapper.Map<UserProfileDto>(user)
    };
}

public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

    if (user == null)
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = "Invalid refresh token."
        };
    }

    if (user.RefreshTokenExpiryTime == null ||
        user.RefreshTokenExpiryTime <= DateTime.UtcNow)
    {
        return new AuthResponseDto
        {
            Success = false,
            Message = "Refresh token has expired."
        };
    }

    string newJwtToken = GenerateJwtToken(user);

    string newRefreshToken = GenerateRefreshToken();

    user.RefreshToken = newRefreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

    await _context.SaveChangesAsync();

    return new AuthResponseDto
    {
        Success = true,
        Message = "Token refreshed successfully.",
        Token = newJwtToken,
        RefreshToken = newRefreshToken,
        Expiration = DateTime.UtcNow.AddHours(2),
        User = _mapper.Map<UserProfileDto>(user)
    };
}

public async Task<bool> LogoutAsync(int userId)
{
    var user = await _context.Users.FindAsync(userId);

    if (user == null)
        return false;

    user.RefreshToken = null;
    user.RefreshTokenExpiryTime = null;

    await _context.SaveChangesAsync();

    return true;
}

private string GenerateJwtToken(User user)
{
    var jwtSettings = _configuration.GetSection("Jwt");

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email)
    };

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

private static string GenerateRefreshToken()
{
    var randomNumber = new byte[64];

    using var rng = RandomNumberGenerator.Create();

    rng.GetBytes(randomNumber);

    return Convert.ToBase64String(randomNumber);
}

}
}