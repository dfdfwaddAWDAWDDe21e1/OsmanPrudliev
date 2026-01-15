using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseApp.API.Data;
using HouseApp.API.DTOs;
using HouseApp.API.Models;

namespace HouseApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HousesController : ControllerBase
{
    private readonly AppDbContext _context;

    public HousesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HouseDto>>> GetHouses()
    {
        var houses = await _context.Houses
            .Include(h => h.HouseTenants)
            .Select(h => new HouseDto
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                LandlordId = h.LandlordId,
                MonthlyRent = h.MonthlyRent,
                UtilitiesCost = h.UtilitiesCost,
                WaterBillCost = h.WaterBillCost,
                MaxOccupants = h.MaxOccupants,
                CurrentOccupants = h.HouseTenants.Count(ht => ht.IsActive),
                CreatedDate = h.CreatedDate
            })
            .ToListAsync();

        return Ok(houses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HouseDto>> GetHouse(int id)
    {
        var house = await _context.Houses
            .Include(h => h.HouseTenants)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (house == null)
        {
            return NotFound();
        }

        var houseDto = new HouseDto
        {
            Id = house.Id,
            Name = house.Name,
            Address = house.Address,
            LandlordId = house.LandlordId,
            MonthlyRent = house.MonthlyRent,
            UtilitiesCost = house.UtilitiesCost,
            WaterBillCost = house.WaterBillCost,
            MaxOccupants = house.MaxOccupants,
            CurrentOccupants = house.HouseTenants.Count(ht => ht.IsActive),
            CreatedDate = house.CreatedDate
        };

        return Ok(houseDto);
    }

    [HttpPost]
    [Authorize(Roles = "Landlord")]
    public async Task<ActionResult<HouseDto>> CreateHouse(HouseDto dto)
    {
        var house = new House
        {
            Name = dto.Name,
            Address = dto.Address,
            LandlordId = dto.LandlordId ?? 0,
            MonthlyRent = dto.MonthlyRent,
            UtilitiesCost = dto.UtilitiesCost,
            WaterBillCost = dto.WaterBillCost,
            MaxOccupants = dto.MaxOccupants,
            CreatedDate = DateTime.UtcNow
        };

        _context.Houses.Add(house);
        await _context.SaveChangesAsync();

        dto.Id = house.Id;
        dto.CreatedDate = house.CreatedDate;
        dto.CurrentOccupants = 0;

        return CreatedAtAction(nameof(GetHouse), new { id = house.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> UpdateHouse(int id, HouseDto dto)
    {
        var house = await _context.Houses.FindAsync(id);
        if (house == null)
        {
            return NotFound();
        }

        house.Name = dto.Name;
        house.Address = dto.Address;
        house.MonthlyRent = dto.MonthlyRent;
        house.UtilitiesCost = dto.UtilitiesCost;
        house.WaterBillCost = dto.WaterBillCost;
        house.MaxOccupants = dto.MaxOccupants;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> DeleteHouse(int id)
    {
        var house = await _context.Houses.FindAsync(id);
        if (house == null)
        {
            return NotFound();
        }

        _context.Houses.Remove(house);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{houseId}/tenants/{studentId}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> AddTenant(int houseId, int studentId)
    {
        var house = await _context.Houses
            .Include(h => h.HouseTenants)
            .FirstOrDefaultAsync(h => h.Id == houseId);

        if (house == null)
        {
            return NotFound("House not found");
        }

        var activeTenantsCount = house.HouseTenants.Count(ht => ht.IsActive);
        if (activeTenantsCount >= house.MaxOccupants)
        {
            return BadRequest("House is at maximum occupancy");
        }

        var existingTenant = await _context.HouseTenants
            .FirstOrDefaultAsync(ht => ht.HouseId == houseId && ht.StudentId == studentId && ht.IsActive);

        if (existingTenant != null)
        {
            return BadRequest("Student is already a tenant");
        }

        var houseTenant = new HouseTenant
        {
            HouseId = houseId,
            StudentId = studentId,
            JoinedDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.HouseTenants.Add(houseTenant);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{houseId}/tenants/{studentId}")]
    [Authorize(Roles = "Landlord")]
    public async Task<IActionResult> RemoveTenant(int houseId, int studentId)
    {
        var houseTenant = await _context.HouseTenants
            .FirstOrDefaultAsync(ht => ht.HouseId == houseId && ht.StudentId == studentId && ht.IsActive);

        if (houseTenant == null)
        {
            return NotFound();
        }

        houseTenant.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{houseId}/messages")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetHouseMessages(int houseId)
    {
        var messages = await _context.Messages
            .Where(m => m.HouseId == houseId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                HouseId = m.HouseId,
                SenderId = m.SenderId,
                SenderName = m.SenderName,
                MessageText = m.MessageText,
                Timestamp = m.Timestamp,
                IsRead = m.IsRead
            })
            .ToListAsync();

        return Ok(messages);
    }
}
