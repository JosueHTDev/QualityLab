using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityLab.API.Data;
using QualityLab.API.Models.DTOs;
using QualityLab.API.Models.Entities;

namespace QualityLab.API.Controllers
{
    /// <summary>
    /// El resultado de una muestra es información sensible: no puede ser
    /// consultado por cualquier usuario autenticado, solo por:
    ///   - ADMIN y SUPERVISOR (control total)
    ///   - El TECNICO asignado a la muestra
    ///   - El CLIENTE dueño de la muestra (solo lectura, y solo si ya está Analizada)
    /// Esta regla se aplica en cada acción, no solo con [Authorize(Roles=...)].
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResultadosController : ControllerBase
    {
        private readonly QualityLabDbContext _context;

        public ResultadosController(QualityLabDbContext context)
        {
            _context = context;
        }

        [HttpGet("muestra/{muestraId:int}")]
        public async Task<ActionResult<IEnumerable<ResultadoResponseDto>>> GetPorMuestra(int muestraId)
        {
            var muestra = await _context.Muestras.FindAsync(muestraId);
            if (muestra is null) return NotFound();

            if (!TienePermisoParaVerResultado(muestra)) return Forbid();

            var resultados = await _context.Resultados
                .Include(r => r.Analisis)
                .Where(r => r.Analisis!.MuestraId == muestraId)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return Ok(resultados);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR,TECNICO")]
        public async Task<ActionResult<ResultadoResponseDto>> Create(ResultadoCreateDto dto)
        {
            var analisis = await _context.Analisis.Include(a => a.Muestra).FirstOrDefaultAsync(a => a.Id == dto.AnalisisId);
            if (analisis is null) return BadRequest(new { mensaje = "El análisis indicado no existe." });

            if (User.IsInRole("TECNICO") && analisis.TecnicoId != ObtenerTecnicoId())
                return Forbid();

            var resultado = new Resultado
            {
                AnalisisId = dto.AnalisisId,
                TecnicoId = analisis.TecnicoId,
                Parametro = dto.Parametro,
                ValorObtenido = dto.ValorObtenido,
                ValorReferencia = dto.ValorReferencia,
                Unidad = dto.Unidad,
                Conforme = dto.Conforme
            };

            _context.Resultados.Add(resultado);
            await _context.SaveChangesAsync();

            var dtoRespuesta = MapToDto(resultado);
            dtoRespuesta.MuestraId = analisis.MuestraId;

            return CreatedAtAction(nameof(GetPorMuestra), new { muestraId = analisis.MuestraId }, dtoRespuesta);
        }

        private bool TienePermisoParaVerResultado(Muestra muestra)
        {
            if (User.IsInRole("ADMIN") || User.IsInRole("SUPERVISOR")) return true;

            if (User.IsInRole("TECNICO"))
                return muestra.TecnicoAsignadoId == ObtenerTecnicoId();

            if (User.IsInRole("CLIENTE"))
                return muestra.ClienteId == ObtenerClienteId();

            return false;
        }

        private int? ObtenerTecnicoId()
        {
            var valor = User.FindFirst("tecnicoId")?.Value;
            return valor is not null && int.TryParse(valor, out var id) ? id : null;
        }

        private int? ObtenerClienteId()
        {
            var valor = User.FindFirst("clienteId")?.Value;
            return valor is not null && int.TryParse(valor, out var id) ? id : null;
        }

        private static ResultadoResponseDto MapToDto(Resultado r) => new()
        {
            Id = r.Id,
            AnalisisId = r.AnalisisId,
            MuestraId = r.Analisis?.MuestraId ?? 0,
            Parametro = r.Parametro,
            ValorObtenido = r.ValorObtenido,
            ValorReferencia = r.ValorReferencia,
            Unidad = r.Unidad,
            Conforme = r.Conforme,
            FechaRegistro = r.FechaRegistro
        };
    }
}
