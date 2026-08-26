using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityLab.API.Data;
using QualityLab.API.Models.DTOs;
using QualityLab.API.Models.Entities;
using QualityLab.API.Models.Enums;
using QualityLab.API.Services;

namespace QualityLab.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly QualityLabDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(QualityLabDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Prueba 1 (login correcto) y Prueba 2 (login incorrecto).
        /// Verifica usuario/clave con BCrypt y, si es válido, entrega un JWT.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == dto.NombreUsuario && u.Activo);

            if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            {
                _logger.LogWarning("Intento de login fallido para usuario {Usuario}", dto.NombreUsuario);
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
            }

            var (token, expiraEn) = _tokenService.GenerarToken(usuario);

            return Ok(new LoginResponseDto
            {
                Token = token,
                ExpiraEn = expiraEn,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol.ToString(),
                ClienteId = usuario.ClienteId,
                TecnicoId = usuario.TecnicoId
            });
        }

        /// <summary>
        /// Registro de nuevas cuentas. Solo ADMIN puede crear usuarios.
        /// </summary>
        [HttpPost("registrar")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> Registrar(RegistrarUsuarioDto dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario))
                return Conflict(new { mensaje = "El nombre de usuario ya existe." });

            if (dto.Rol == RolUsuario.CLIENTE && dto.ClienteId is null)
                return BadRequest(new { mensaje = "ClienteId es requerido para el rol CLIENTE." });

            if (dto.Rol == RolUsuario.TECNICO && dto.TecnicoId is null)
                return BadRequest(new { mensaje = "TecnicoId es requerido para el rol TECNICO." });

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = dto.Rol,
                ClienteId = dto.ClienteId,
                TecnicoId = dto.TecnicoId
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { }, new { mensaje = "Usuario creado correctamente.", usuario.Id });
        }
    }
}
