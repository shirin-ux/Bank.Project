using Common;
using LoanGateway.Auth.Application;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LoanGateway.Auth.Infrastructure.Communication;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IOptions<JwtOptions> _options;


    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public JwtTokenService(IOptions<JwtOptions> options, IRefreshTokenRepository refreshTokenRepository)
    {
        _options = options;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<JwtTokenPair> GenerateTokensAsync(User user, CancellationToken ct)
    {
        try
        {
            var now = DateTime.UtcNow;
            var jwtId = Guid.NewGuid();


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId.ToString()),
                new Claim(JwtRegisteredClaimNames.PhoneNumber, user.MobileNumber),
                new Claim("isActive", user.IsActive ? "true" : "false")
            };
            if (!string.IsNullOrWhiteSpace(user.NationalCode))
            {
                new Claim("national_code", user.NationalCode);
            }

            var accessTokenExpires = now.AddMinutes(_options.Value.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: _options.Value.Issuer,
                audience: _options.Value.Audience,
                claims: claims,
                notBefore: now,
                expires: accessTokenExpires,
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);


            var refreshToken = GenerateSecureRandomToken(64);
            var refreshTokenExpires = now.AddDays(_options.Value.RefreshTokenDays);
    


            var refreshEntity = new RefreshTokens
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                JwtId = jwtId,
                RefreshToken = refreshToken,     
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessTokenExpires,
                ExpiresAtUtc = refreshTokenExpires,
            
            };

            await _refreshTokenRepository.InsertAsync(refreshEntity, ct);

            return new JwtTokenPair
            {
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessTokenExpires,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAtUtc = refreshTokenExpires,
                JwtId = jwtId
            };

        }
        catch (Exception ex)
        {

            throw;
        }

    }

    private static string GenerateSecureRandomToken(int byteLength)
    {
        var bytes = new byte[byteLength];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static byte[] HashToken(string token)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(token));
    }



   public async Task<JwtTokenPair> GetTokenPairAsync(Guid refreshTokenId, CancellationToken ct)
    {
        var result =await _refreshTokenRepository.GetTokenPairAsync(refreshTokenId, ct);
        if (result == null)
            throw new InvalidOperationException("توکن جایگزین یافت نشد.");

        return new JwtTokenPair
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            AccessTokenExpiresAtUtc = result.AccessTokenExpiresAtUtc,
            RefreshTokenExpiresAtUtc = result.ExpiresAtUtc
        };
    }
}
