using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QualityLab.API.Models.Entities;

namespace QualityLab.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string token, DateTime expiraEn) GenerarToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Rol.ToString())
            };

            if (usuario.ClienteId.HasValue)
                claims.Add(new Claim("clienteId", usuario.ClienteId.Value.ToString()));

            if (usuario.TecnicoId.HasValue)
                claims.Add(new Claim("tecnicoId", usuario.TecnicoId.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiraEn = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiraEn,
                signingCredentials: credenciales
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
        }
    }
}
