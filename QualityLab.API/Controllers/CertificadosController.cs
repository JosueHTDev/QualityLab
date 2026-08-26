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
    [Authorize]
    public class CertificadosController : ControllerBase
    {
        private readonly QualityLabDbContext _context;
        private readonly ICertificadoService _certificadoService;

        public CertificadosController(QualityLabDbContext context, ICertificadoService certificadoService)
        {
            _context = context;
            _certificadoService = certificadoService;
        }

        /// <summary>
        /// Emite el certificado de una muestra ya analizada. Solo ADMIN/SUPERVISOR.
        /// </summary>
        [HttpPost("muestra/{muestraId:int}/emitir")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<CertificadoResponseDto>> Emitir(int muestraId)
        {
            var muestra = await _context.Muestras
                .Include(m => m.Certificado)
                .Include(m => m.Analisis).ThenInclude(a => a.Resultados)
                .FirstOrDefaultAsync(m => m.Id == muestraId);

            if (muestra is null) return NotFound();
            if (muestra.Certificado is not null)
                return Conflict(new { mensaje = "Esta muestra ya tiene un certificado emitido." });

            if (muestra.Estado != EstadoMuestra.Analizada)
                return BadRequest(new { mensaje = "La muestra debe estar Analizada antes de emitir el certificado." });

            var resultados = muestra.Analisis.SelectMany(a => a.Resultados).ToList();
            var codigo = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{muestraId:D5}";

            var (nombreArchivo, contenido) = _certificadoService.GenerarContenidoCertificado(muestra, resultados, codigo);

            var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? "0");

            var certificado = new Certificado
            {
                MuestraId = muestraId,
                CodigoCertificado = codigo,
                Estado = EstadoCertificado.Emitido,
                ContenidoArchivo = contenido,
                NombreArchivo = nombreArchivo,
                GeneradoPorUsuarioId = usuarioId
            };

            _context.Certificados.Add(certificado);
            muestra.Estado = EstadoMuestra.CertificadoEmitido;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(certificado));
        }

        /// <summary>
        /// Descarga del certificado (Web: "Descargar certificado"). Mismo control
        /// de acceso que los resultados: solo el cliente dueño, técnico asignado o staff.
        /// </summary>
        [HttpGet("muestra/{muestraId:int}/descargar")]
        public async Task<IActionResult> Descargar(int muestraId)
        {
            var muestra = await _context.Muestras
                .Include(m => m.Certificado)
                .FirstOrDefaultAsync(m => m.Id == muestraId);

            if (muestra?.Certificado is null) return NotFound();

            if (!TienePermiso(muestra)) return Forbid();

            return File(muestra.Certificado.ContenidoArchivo, "text/plain", muestra.Certificado.NombreArchivo);
        }

        private bool TienePermiso(Muestra muestra)
        {
            if (User.IsInRole("ADMIN") || User.IsInRole("SUPERVISOR")) return true;

            if (User.IsInRole("CLIENTE"))
            {
                var clienteId = User.FindFirst("clienteId")?.Value;
                return clienteId is not null && int.Parse(clienteId) == muestra.ClienteId;
            }

            if (User.IsInRole("TECNICO"))
            {
                var tecnicoId = User.FindFirst("tecnicoId")?.Value;
                return tecnicoId is not null && int.Parse(tecnicoId) == muestra.TecnicoAsignadoId;
            }

            return false;
        }

        private static CertificadoResponseDto MapToDto(Certificado c) => new()
        {
            Id = c.Id,
            MuestraId = c.MuestraId,
            CodigoCertificado = c.CodigoCertificado,
            FechaEmision = c.FechaEmision,
            Estado = c.Estado.ToString()
        };
    }
}
