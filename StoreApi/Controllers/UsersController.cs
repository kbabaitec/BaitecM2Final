using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Data;
using StoreApi.Models;
namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;
    // GET api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserDto(u.Id, u.Username, u.FullName, u.Role))
            .ToListAsync();
        return Ok(users);
    }
    // POST api/users
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password are required.");
        if (req.Username.Trim().Length > 30)
            return BadRequest("Username can be at most 30 characters.");
        if (req.Role != "Admin" && req.Role != "Cashier")
            return BadRequest("Role must be Admin or Cashier.");
        if (await _db.Users.AnyAsync(u => u.Username == req.Username.Trim()))
            return Conflict($"Username '{req.Username}' is already taken.");
        var user = new AppUser
        {
            Username = req.Username.Trim(),
            FullName = req.FullName.Trim(),
            Role = req.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new UserDto(user.Id, user.Username, user.FullName, user.Role));
    }
    // DELETE api/users/3
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound("User not found.");
        if (user.Username == "admin")
            return BadRequest("The main admin account cannot be deleted.");
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}