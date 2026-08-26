using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualityLab.API.Data;
using QualityLab.API.Models.DTOs;
using QualityLab.API.Models.Entities;

namespace QualityLab.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LotesController : ControllerBase
    {
        private readonly QualityLabDbContext _context;

        public LotesController(QualityLabDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISOR,CLIENTE")]
        public async Task<ActionResult<IEnumerable<LoteResponseDto>>> GetAll()
        {
            var query = _context.Lotes.Include(l => l.Cliente).AsQueryable();

            // Un cliente solo ve sus propios lotes
            if (User.IsInRole("CLIENTE"))
            {
                var clienteId = ObtenerClienteIdDelToken();
                query = query.Where(l => l.ClienteId == clienteId);
            }

            var lotes = await query.Select(l => MapToDto(l)).ToListAsync();
            return Ok(lotes);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR,CLIENTE")]
        public async Task<ActionResult<LoteResponseDto>> GetById(int id)
        {
            var lote = await _context.Lotes.Include(l => l.Cliente).FirstOrDefaultAsync(l => l.Id == id);
            if (lote is null) return NotFound();

            if (User.IsInRole("CLIENTE") && lote.ClienteId != ObtenerClienteIdDelToken())
                return Forbid();

            return Ok(MapToDto(lote));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<LoteResponseDto>> Create(LoteCreateDto dto)
        {
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId);
            if (!clienteExiste) return BadRequest(new { mensaje = "El cliente indicado no existe." });

            var lote = new Lote
            {
                Codigo = dto.Codigo,
                ClienteId = dto.ClienteId,
                Descripcion = dto.Descripcion
            };

            _context.Lotes.Add(lote);
            await _context.SaveChangesAsync();

            var creado = await _context.Lotes.Include(l => l.Cliente).FirstAsync(l => l.Id == lote.Id);
            return CreatedAtAction(nameof(GetById), new { id = lote.Id }, MapToDto(creado));
        }

        private int? ObtenerClienteIdDelToken()
        {
            var claim = User.FindFirst("clienteId")?.Value;
            return claim is not null && int.TryParse(claim, out var id) ? id : null;
        }

        private static LoteResponseDto MapToDto(Lote l) => new()
        {
            Id = l.Id,
            Codigo = l.Codigo,
            ClienteId = l.ClienteId,
            ClienteRazonSocial = l.Cliente?.RazonSocial,
            FechaRecepcion = l.FechaRecepcion,
            Descripcion = l.Descripcion,
            Estado = l.Estado.ToString()
        };
    }
}
