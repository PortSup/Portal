using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Mail;

[ApiController]
[Route("api/[controller]")]
public class BuyerDashboardController : ControllerBase
{
    private readonly string _connectionString;

    public BuyerDashboardController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpPost("InsertBuyer")]
    public async Task<IActionResult> InsertBuyer([FromBody] BuyerModel buyer)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Save the buyer
            string queryBuyer = "INSERT INTO dbo.cfgUser (UserName, FirstNames, LastNames, MobileNumber, EmailAddress) " +
                                "OUTPUT INSERTED.UserID VALUES (@UserName, @FirstNames, @LastNames, @MobileNumber, @EmailAddress)";

            int userId; // Variable to store the generated UserID

            using (var command = new SqlCommand(queryBuyer, connection))
            {
                command.Parameters.AddWithValue("@UserName", buyer.UserName);
                command.Parameters.AddWithValue("@FirstNames", buyer.FirstName);
                command.Parameters.AddWithValue("@LastNames", buyer.LastName);
                command.Parameters.AddWithValue("@MobileNumber", buyer.cellNumber);
                command.Parameters.AddWithValue("@EmailAddress", buyer.Email);

                // Execute the command and retrieve the UserID
                userId = (int)await command.ExecuteScalarAsync();
            }

            // Insert into cfgUserRoles
            string queryUserRole = "INSERT INTO dbo.cfgUserRoles (UserID, RoleID) VALUES (@UserID, @RoleID)";
            using (var command = new SqlCommand(queryUserRole, connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@RoleID", 1); 

                await command.ExecuteNonQueryAsync();
            }
        }

        return Ok("Buyer data inserted successfully.");
    }

    [HttpGet("GetDashboardData")]
    public async Task<IActionResult> GetDashboardData()
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var dashboardData = new
                {
                    VendorCount = await GetVendorCount(connection),
                    ProcessedVendors = await GetProcessedVendorsCount(connection),
                    Announcements = await GetAnnouncements(connection),
                    FilesUploaded = await GetFilesUploadedCount(connection),
                    DataVerification = await GetDataVerificationCount(connection),
                    NewVendors = await GetNewVendorsCount(connection),
                    Buyers = await GetBuyersCount(connection)
                };

                return Ok(dashboardData);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("GetBuyer")]
    public async Task<IActionResult> GetBuyer()
    {
        // Collect all users
        var usersQuery = "SELECT UserID, FirstNames, LastNames, EmailAddress, MobileNumber FROM cfgUser";
        var roleIds = new List<int> { 1, 3, 4, 5 }; // RoleIDs to check [Catered for buyers]

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            //  Get all users
            var users = new List<UserDto>(); 
            using (var command = new SqlCommand(usersQuery, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new UserDto
                        {
                            UserID = reader.GetInt32(0),
                            FirstNames = reader.GetString(1),
                            LastNames = reader.GetString(2),
                            EmailAddress = reader.GetString(3),
                            MobileNumber = reader.GetString(4)
                        });
                    }
                }
            }

            // Get UserIDs linked to the specified RoleIDs
            var userIdsWithRoles = new List<int>();
            var roleIdsQuery = "SELECT UserID FROM cfgUserRoles WHERE RoleID IN (@RoleID1, @RoleID2, @RoleID3, @RoleID4)";

            using (var command = new SqlCommand(roleIdsQuery, connection))
            {
                command.Parameters.AddWithValue("@RoleID1", roleIds[0]);
                command.Parameters.AddWithValue("@RoleID2", roleIds[1]);
                command.Parameters.AddWithValue("@RoleID3", roleIds[2]);
                command.Parameters.AddWithValue("@RoleID4", roleIds[3]);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        userIdsWithRoles.Add(reader.GetInt32(0));
                    }
                }
            }

            // Filter users based on UserIDs with roles
            var filteredUsers = users.Where(u => userIdsWithRoles.Contains(u.UserID)).ToList();

            // Check if any users matched the criteria
            if (!filteredUsers.Any())
            {
                return NotFound(new { message = "No users found with the specified role IDs." });
            }

            return Ok(filteredUsers); 
        }
    }

    private async Task<int> GetRole(SqlConnection connection)
    {
        using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Vendor", connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<int> GetVendorCount(SqlConnection connection)
    {
        using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Vendor", connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<int> GetProcessedVendorsCount(SqlConnection connection)
    {
        using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Vendor WHERE SAPProcessed = 1", connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<List<object>> GetAnnouncements(SqlConnection connection)
    {
        var announcements = new List<object>();
        using var command = new SqlCommand(
            "SELECT Title, Description FROM Announcement WHERE ExpiryDate IS NULL OR ExpiryDate > GETDATE()",
            connection
        );

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            announcements.Add(new
            {
                Title = reader["Title"].ToString(),
                Description = reader["Description"].ToString()
            });
        }

        return announcements;
    }

    private async Task<int> GetFilesUploadedCount(SqlConnection connection)
    {
        using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Attachment", connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<object> GetDataVerificationCount(SqlConnection connection)
    {
        using var command = new SqlCommand(
            "SELECT " +
            "(SELECT COUNT(*) FROM dbo.Vendor WHERE SARSVerified = 1 AND CIPCVerified = 1) as Verified, " +
            "(SELECT COUNT(*) FROM dbo.Vendor) as Total",
            connection
        );

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new
            {
                Verified = reader.GetInt32(0),
                Total = reader.GetInt32(1)
            };
        }

        return new { Verified = 0, Total = 0 };
    }

    private async Task<int> GetNewVendorsCount(SqlConnection connection)
    {
        using var command = new SqlCommand(
            "SELECT COUNT(*) FROM dbo.Vendor WHERE CreatedDateTime >= DATEADD(day, -2, GETDATE())",
            connection
        );
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<object> GetBuyersCount(SqlConnection connection)
    {
        using var command = new SqlCommand(
            "SELECT " +
            "(SELECT COUNT(*) FROM dbo.Buyer) as Total, " +
            "(SELECT COUNT(*) FROM dbo.Buyer WHERE Status = 'Approved') as Approved",
            connection
        );

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new
            {
                Total = reader.GetInt32(0),
                Approved = reader.GetInt32(1)
            };
        }

        return new { Total = 0, Approved = 0 };
    }
}

public class UserDto
{
    public int UserID { get; set; }
    public string FirstNames { get; set; }
    public string LastNames { get; set; }
    public string EmailAddress { get; set; }
    public string MobileNumber { get; set; }
}

public class BuyerModel
{
    public int UserName { get; set; }
    public int FirstName { get; set; }
    public string LastName { get; set; }
    public string cellNumber { get; set; }
    public string Email { get; set; }
    public string UserID { get; set; }
    public string Role { get; set; }
}

//1=admin ; 3=approver; 4=read-only user; 5=support