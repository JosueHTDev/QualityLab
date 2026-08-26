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
    [Authorize] // requiere token válido para cualquier acción de este controlador
    public class ClientesController : ControllerBase
    {
        private readonly QualityLabDbContext _context;

        public ClientesController(QualityLabDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> GetAll()
        {
            var clientes = await _context.Clientes
                .Select(c => MapToDto(c))
                .ToListAsync();

            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "ADMIN,SUPERVISOR,CLIENTE")]
        public async Task<ActionResult<ClienteResponseDto>> GetById(int id)
        {
            // Un CLIENTE solo puede consultar su propia ficha
            if (User.IsInRole("CLIENTE") && !EsPropioCliente(id))
                return Forbid();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null) return NotFound();

            return Ok(MapToDto(cliente));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ClienteResponseDto>> Create(ClienteCreateDto dto)
        {
            if (await _context.Clientes.AnyAsync(c => c.RUC == dto.RUC))
                return Conflict(new { mensaje = "Ya existe un cliente con ese RUC." });

            var cliente = new Cliente
            {
                RazonSocial = dto.RazonSocial,
                RUC = dto.RUC,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, MapToDto(cliente));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> Update(int id, ClienteCreateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null) return NotFound();

            cliente.RazonSocial = dto.RazonSocial;
            cliente.RUC = dto.RUC;
            cliente.Email = dto.Email;
            cliente.Telefono = dto.Telefono;
            cliente.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> Deactivate(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null) return NotFound();

            cliente.Activo = false; // baja lógica, no se elimina físicamente
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool EsPropioCliente(int clienteId)
        {
            var claim = User.FindFirst("clienteId")?.Value;
            return claim is not null && int.TryParse(claim, out var idClaim) && idClaim == clienteId;
        }

        private static ClienteResponseDto MapToDto(Cliente c) => new()
        {
            Id = c.Id,
            RazonSocial = c.RazonSocial,
            RUC = c.RUC,
            Email = c.Email,
            Telefono = c.Telefono,
            Direccion = c.Direccion,
            FechaRegistro = c.FechaRegistro,
            Activo = c.Activo
        };
    }
}
