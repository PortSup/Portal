using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;

[Route("api/[controller]")]
[ApiController]
public class AnnouncementController : ControllerBase
{
    private readonly string _connectionString;

    public AnnouncementController(IConfiguration configuration)
    {
         _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateAnnouncement([FromBody] Announcement announcement)
    {
        try
        {
            if (announcement == null) return BadRequest("Invalid input.");

            // DB Insertion Logic
            var query = @"
                INSERT INTO [dbo].[mdAnnouncements]
                ([Title], [Description], [CRUDDatetIME], [ExpiryDate], [IsActive])
                VALUES (@Title, @Description, @CRUDDateTime, @ExpiryDate, @IsActive)";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(query, new
                {
                    Title = announcement.Title,
                    Description = announcement.Description,
                    CRUDDateTime = DateTime.UtcNow,
                    ExpiryDate = announcement.ExpiryDate,
                    IsActive = announcement.IsActive
                });
            }

            return Ok("Announcement added successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }
}

// Model Class
public class Announcement
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}
