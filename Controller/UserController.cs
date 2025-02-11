using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly string _connectionString;

    public UserController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserModel model)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
           var sql = @"
                INSERT INTO Contact (
                   VendorID, PrimaryContactEmail, PrimaryContactName, PrimaryContactSurname, SecondaryContactCell, CreatedDateTime
                ) 
                VALUES (
                    @VendorID, @PrimaryContactEmail, @PrimaryContactName, @PrimaryContactSurname, @SecondaryContactCell, GETDATE()
                )";
            await connection.ExecuteAsync(sql, model);
        }

        return Ok(new { message = "Personal deatails saved successfully" });
    }

}



public class UserModel
{
    public int VendorID {get; set; }
    public string PrimaryContactEmail { get; set; }
    public string PrimaryContactName { get; set; }
    public string PrimaryContactSurname { get; set; }
    public int SecondaryContactCell { get; set; }
}