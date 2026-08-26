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
    [Authorize]
    public class MuestrasController : ControllerBase
    {
        private readonly QualityLabDbContext _context;

        public MuestrasController(QualityLabDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista de muestras, filtrada según el rol:
        /// ADMIN/SUPERVISOR ven todo, CLIENTE solo las suyas, TECNICO solo las asignadas a él.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MuestraResponseDto>>> GetAll()
        {
            var query = _context.Muestras.Include(m => m.TecnicoAsignado).AsQueryable();

            if (User.IsInRole("CLIENTE"))
            {
                var clienteId = ObtenerClaimInt("clienteId");
                query = query.Where(m => m.ClienteId == clienteId);
            }
            else if (User.IsInRole("TECNICO"))
            {
                var tecnicoId = ObtenerClaimInt("tecnicoId");
                query = query.Where(m => m.TecnicoAsignadoId == tecnicoId);
            }
            // ADMIN y SUPERVISOR ven todas, sin filtro adicional

            var muestras = await query.Select(m => MapToDto(m)).ToListAsync();
            return Ok(muestras);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MuestraResponseDto>> GetById(int id)
        {
            var muestra = await _context.Muestras.Include(m => m.TecnicoAsignado).FirstOrDefaultAsync(m => m.Id == id);
            if (muestra is null) return NotFound();

            if (!TienePermisoSobreMuestra(muestra)) return Forbid();

            return Ok(MapToDto(muestra));
        }

        /// <summary>
        /// Consulta puntual de estado, pensada para el portal web del cliente
        /// (Web: "Consultar estado").
        /// </summary>
        [HttpGet("{id:int}/estado")]
        [Authorize(Roles = "ADMIN,SUPERVISOR,CLIENTE")]
        public async Task<ActionResult<EstadoMuestraDto>> GetEstado(int id)
        {
            var muestra = await _context.Muestras.Include(m => m.Certificado).FirstOrDefaultAsync(m => m.Id == id);
            if (muestra is null) return NotFound();

            if (!TienePermisoSobreMuestra(muestra)) return Forbid();

            return Ok(new EstadoMuestraDto
            {
                MuestraId = muestra.Id,
                Codigo = muestra.Codigo,
                Estado = muestra.Estado.ToString(),
                TieneCertificado = muestra.Certificado is not null
            });
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<MuestraResponseDto>> Create(MuestraCreateDto dto)
        {
            var lote = await _context.Lotes.FindAsync(dto.LoteId);
            if (lote is null) return BadRequest(new { mensaje = "El lote indicado no existe." });

            var muestra = new Muestra
            {
                Codigo = dto.Codigo,
                LoteId = dto.LoteId,
                ClienteId = dto.ClienteId,
                TipoProducto = dto.TipoProducto,
                Estado = EstadoMuestra.Registrada
            };

            _context.Muestras.Add(muestra);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = muestra.Id }, MapToDto(muestra));
        }

        /// <summary>
        /// Paso "Asignación técnico" del flujo del laboratorio (WinForms).
        /// </summary>
        [HttpPost("{id:int}/asignar-tecnico")]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult> AsignarTecnico(int id, AsignarTecnicoDto dto)
        {
            var muestra = await _context.Muestras.FindAsync(id);
            if (muestra is null) return NotFound();

            var tecnico = await _context.Tecnicos.FindAsync(dto.TecnicoId);
            if (tecnico is null || !tecnico.Activo)
                return BadRequest(new { mensaje = "El técnico indicado no existe o está inactivo." });

            muestra.TecnicoAsignadoId = dto.TecnicoId;
            muestra.FechaAsignacion = DateTime.UtcNow;
            muestra.Estado = EstadoMuestra.Asignada;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Lista de muestras asignadas al técnico autenticado (Móvil: "Consultar muestras asignadas").
        /// </summary>
        [HttpGet("asignadas-a-mi")]
        [Authorize(Roles = "TECNICO")]
        public async Task<ActionResult<IEnumerable<MuestraResponseDto>>> GetAsignadasAMi()
        {
            var tecnicoId = ObtenerClaimInt("tecnicoId");

            var muestras = await _context.Muestras
                .Include(m => m.TecnicoAsignado)
                .Where(m => m.TecnicoAsignadoId == tecnicoId)
                .Select(m => MapToDto(m))
                .ToListAsync();

            return Ok(muestras);
        }

        private bool TienePermisoSobreMuestra(Muestra muestra)
        {
            if (User.IsInRole("ADMIN") || User.IsInRole("SUPERVISOR")) return true;
            if (User.IsInRole("CLIENTE")) return muestra.ClienteId == ObtenerClaimInt("clienteId");
            if (User.IsInRole("TECNICO")) return muestra.TecnicoAsignadoId == ObtenerClaimInt("tecnicoId");
            return false;
        }

        private int? ObtenerClaimInt(string nombreClaim)
        {
            var valor = User.FindFirst(nombreClaim)?.Value;
            return valor is not null && int.TryParse(valor, out var id) ? id : null;
        }

        private static MuestraResponseDto MapToDto(Muestra m) => new()
        {
            Id = m.Id,
            Codigo = m.Codigo,
            LoteId = m.LoteId,
            ClienteId = m.ClienteId,
            TipoProducto = m.TipoProducto,
            FechaRecepcion = m.FechaRecepcion,
            Estado = m.Estado.ToString(),
            TecnicoAsignadoId = m.TecnicoAsignadoId,
            TecnicoAsignadoNombre = m.TecnicoAsignado is not null
                ? $"{m.TecnicoAsignado.Nombres} {m.TecnicoAsignado.Apellidos}"
                : null
        };
    }
}
