using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityLab.API.Data;
using QualityLab.API.Models.DTOs;
using QualityLab.API.Models.Entities;
using QualityLab.API.Models.Enums;

namespace QualityLab.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN,SUPERVISOR,TECNICO")]
    public class AnalisisController : ControllerBase
    {
        private readonly QualityLabDbContext _context;

        public AnalisisController(QualityLabDbContext context)
        {
            _context = context;
        }

        [HttpGet("muestra/{muestraId:int}")]
        public async Task<ActionResult<IEnumerable<AnalisisResponseDto>>> GetPorMuestra(int muestraId)
        {
            var muestra = await _context.Muestras.FindAsync(muestraId);
            if (muestra is null) return NotFound();

            if (User.IsInRole("TECNICO") && muestra.TecnicoAsignadoId != ObtenerTecnicoId())
                return Forbid();

            var analisis = await _context.Analisis
                .Where(a => a.MuestraId == muestraId)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return Ok(analisis);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<AnalisisResponseDto>> Create(AnalisisCreateDto dto)
        {
            var muestra = await _context.Muestras.FindAsync(dto.MuestraId);
            if (muestra is null) return BadRequest(new { mensaje = "La muestra indicada no existe." });

            var analisis = new Analisis
            {
                MuestraId = dto.MuestraId,
                TecnicoId = dto.TecnicoId,
                TipoAnalisis = dto.TipoAnalisis,
                Estado = EstadoAnalisis.Pendiente
            };

            _context.Analisis.Add(analisis);

            muestra.Estado = EstadoMuestra.EnAnalisis;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPorMuestra), new { muestraId = dto.MuestraId }, MapToDto(analisis));
        }

        /// <summary>
        /// El técnico marca su análisis como completado. Esto no expone el resultado:
        /// el detalle numérico vive en ResultadosController, con su propio control de acceso.
        /// </summary>
        [HttpPut("{id:int}/completar")]
        public async Task<ActionResult> Completar(int id, CompletarAnalisisDto dto)
        {
            var analisis = await _context.Analisis.Include(a => a.Muestra).FirstOrDefaultAsync(a => a.Id == id);
            if (analisis is null) return NotFound();

            if (User.IsInRole("TECNICO") && analisis.TecnicoId != ObtenerTecnicoId())
                return Forbid();

            analisis.Estado = EstadoAnalisis.Completado;
            analisis.FechaFin = DateTime.UtcNow;
            analisis.Observaciones = dto.Observaciones;

            if (analisis.Muestra is not null)
                analisis.Muestra.Estado = EstadoMuestra.Analizada;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int? ObtenerTecnicoId()
        {
            var valor = User.FindFirst("tecnicoId")?.Value;
            return valor is not null && int.TryParse(valor, out var id) ? id : null;
        }

        private static AnalisisResponseDto MapToDto(Analisis a) => new()
        {
            Id = a.Id,
            MuestraId = a.MuestraId,
            TipoAnalisis = a.TipoAnalisis,
            FechaInicio = a.FechaInicio,
            FechaFin = a.FechaFin,
            Estado = a.Estado.ToString(),
            Observaciones = a.Observaciones
        };
    }
}
