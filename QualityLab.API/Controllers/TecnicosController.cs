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
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    public class TecnicosController : ControllerBase
    {
        private readonly QualityLabDbContext _context;

        public TecnicosController(QualityLabDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TecnicoResponseDto>>> GetAll()
        {
            var tecnicos = await _context.Tecnicos.Select(t => MapToDto(t)).ToListAsync();
            return Ok(tecnicos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TecnicoResponseDto>> GetById(int id)
        {
            var tecnico = await _context.Tecnicos.FindAsync(id);
            if (tecnico is null) return NotFound();
            return Ok(MapToDto(tecnico));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<TecnicoResponseDto>> Create(TecnicoCreateDto dto)
        {
            var tecnico = new Tecnico
            {
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Especialidad = dto.Especialidad,
                Email = dto.Email,
                Telefono = dto.Telefono
            };

            _context.Tecnicos.Add(tecnico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tecnico.Id }, MapToDto(tecnico));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> Update(int id, TecnicoCreateDto dto)
        {
            var tecnico = await _context.Tecnicos.FindAsync(id);
            if (tecnico is null) return NotFound();

            tecnico.Nombres = dto.Nombres;
            tecnico.Apellidos = dto.Apellidos;
            tecnico.Especialidad = dto.Especialidad;
            tecnico.Email = dto.Email;
            tecnico.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> Deactivate(int id)
        {
            var tecnico = await _context.Tecnicos.FindAsync(id);
            if (tecnico is null) return NotFound();

            tecnico.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static TecnicoResponseDto MapToDto(Tecnico t) => new()
        {
            Id = t.Id,
            Nombres = t.Nombres,
            Apellidos = t.Apellidos,
            Especialidad = t.Especialidad,
            Email = t.Email,
            Activo = t.Activo
        };
    }
}
