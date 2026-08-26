using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityLab.API.Data;
using QualityLab.API.Models.DTOs;
using QualityLab.API.Models.Entities;

namespace QualityLab.API.Controllers
{
    /// <summary>
    /// Endpoints usados por la app móvil .NET MAUI: registrar avance y registrar
    /// incidencia. Soportan sincronización posterior a una pérdida de conexión
    /// (Prueba 7 y Prueba 8): el cliente puede enviar "IdLocalOrigen" (un GUID
    /// generado offline) para que, si reintenta el envío, el servidor no duplique
    /// el registro (operación idempotente).
    /// </summary>
    [ApiController]
    [Route("api/tecnico")]
    [Authorize(Roles = "TECNICO")]
    public class TecnicoOperacionesController : ControllerBase
    {
        private readonly QualityLabDbContext _context;

        public TecnicoOperacionesController(QualityLabDbContext context)
        {
            _context = context;
        }

        [HttpPost("avances")]
        public async Task<ActionResult> RegistrarAvance(RegistrarAvanceDto dto)
        {
            var tecnicoId = ObtenerTecnicoId();
            if (tecnicoId is null) return Forbid();

            var muestra = await _context.Muestras.FindAsync(dto.MuestraId);
            if (muestra is null) return BadRequest(new { mensaje = "La muestra indicada no existe." });

            if (muestra.TecnicoAsignadoId != tecnicoId)
                return Forbid();

            // Idempotencia: si ya se sincronizó este registro offline, no duplicar
            if (dto.IdLocalOrigen.HasValue)
            {
                var existente = await _context.Avances
                    .FirstOrDefaultAsync(a => a.IdLocalOrigen == dto.IdLocalOrigen);
                if (existente is not null)
                    return Ok(new { mensaje = "Avance ya sincronizado previamente.", id = existente.Id });
            }

            var avance = new AvanceMuestra
            {
                MuestraId = dto.MuestraId,
                TecnicoId = tecnicoId.Value,
                Descripcion = dto.Descripcion,
                PorcentajeAvance = dto.PorcentajeAvance,
                IdLocalOrigen = dto.IdLocalOrigen,
                FechaCreacionLocal = dto.FechaCreacionLocal
            };

            _context.Avances.Add(avance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAvancesPorMuestra), new { muestraId = dto.MuestraId }, new { id = avance.Id });
        }

        [HttpGet("avances/muestra/{muestraId:int}")]
        public async Task<ActionResult> GetAvancesPorMuestra(int muestraId)
        {
            var avances = await _context.Avances
                .Where(a => a.MuestraId == muestraId)
                .OrderByDescending(a => a.FechaRegistro)
                .ToListAsync();

            return Ok(avances);
        }

        [HttpPost("incidencias")]
        public async Task<ActionResult> RegistrarIncidencia(RegistrarIncidenciaDto dto)
        {
            var tecnicoId = ObtenerTecnicoId();
            if (tecnicoId is null) return Forbid();

            var muestra = await _context.Muestras.FindAsync(dto.MuestraId);
            if (muestra is null) return BadRequest(new { mensaje = "La muestra indicada no existe." });

            if (muestra.TecnicoAsignadoId != tecnicoId)
                return Forbid();

            if (dto.IdLocalOrigen.HasValue)
            {
                var existente = await _context.Incidencias
                    .FirstOrDefaultAsync(i => i.IdLocalOrigen == dto.IdLocalOrigen);
                if (existente is not null)
                    return Ok(new { mensaje = "Incidencia ya sincronizada previamente.", id = existente.Id });
            }

            var incidencia = new Incidencia
            {
                MuestraId = dto.MuestraId,
                TecnicoId = tecnicoId.Value,
                Descripcion = dto.Descripcion,
                IdLocalOrigen = dto.IdLocalOrigen,
                FechaCreacionLocal = dto.FechaCreacionLocal
            };

            _context.Incidencias.Add(incidencia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAvancesPorMuestra), new { muestraId = dto.MuestraId }, new { id = incidencia.Id });
        }

        private int? ObtenerTecnicoId()
        {
            var valor = User.FindFirst("tecnicoId")?.Value;
            return valor is not null && int.TryParse(valor, out var id) ? id : null;
        }
    }
}
