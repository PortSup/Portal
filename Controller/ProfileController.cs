using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;


[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly string _connectionString;

    public ProfileController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpPost("LoadProfileData")]
    public async Task<IActionResult> GetProfileData()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            int supplierId = GetSupplierIdByApplication(connection, 1 );
            if (supplierId == null) return NotFound("Supplier not found.");

            //Get UserId - using that SupplierID
            int userId = GetUserIdBySupplierID(connection, supplierId);
            if (userId == null) return NotFound("User not found");

            var query = "SELECT FirstNames, LastNames, MobileNumber, EmailAddress FROM cfgUser WHERE UserID = @UserID";
            var command = new SqlCommand(query, connection);

            var ProfileData = new List<ProfileModel>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ProfileData.Add(new ProfileModel
                    {
                        FirstName = reader.GetString(0),
                        LastName = reader.GetString(1),
                        cellNumber = reader.GetString(2),
                        Email = reader.GetString(3),
                    });
                }
            }

            return Ok(ProfileData);
        }
    }


    [HttpPost("insertProfileData")]
    public IActionResult InsertProfileData([FromBody] ProfileModel profile)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Step 1: Get SupplierID using AppId
            int supplierId = GetSupplierIdByApplication(connection, 1 );
            if (supplierId == null) return NotFound("Supplier not found.");

            //Get UserId - using that SupplierID
            int userId = GetUserIdBySupplierID(connection, supplierId);
            if (userId == null) return NotFound("User not found");

            //use supplier id to up date the mdSupplier table 

            string updateSupplier = @"
                UPDATE [dbo].[mdSuppliers]
                SET 
                    [SupplierName] = @SupplierName,
                    [SupplierAddress] = @SupplierAddress,
                    WHERE [SupplierID] = @SupplierID";

            using (var command = new SqlCommand(updateSupplier, connection))
            {
                command.Parameters.AddWithValue("@SupplierName", profile.CompanyName);
                command.Parameters.AddWithValue("@EmailAddress", profile.Email);
                command.Parameters.AddWithValue("@SupplierID", supplierId);

                int rowsAffected = command.ExecuteNonQuery();
            }

            //Update the following if userId found 
            string updateQuery = @"
                UPDATE [dbo].[cfgUser]
                SET 
                    [FirstNames] = @FirstNames,
                    [LastNames] = @LastNames,
                    [MobileNumber] = @MobileNumber,
                    [EmailAddress] = @EmailAddress
                WHERE [UserID] = @UserId";

            using (var command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@FirstNames", profile.FirstName);
                command.Parameters.AddWithValue("@LastNames", profile.LastName);
                command.Parameters.AddWithValue("@MobileNumber", profile.cellNumber);
                command.Parameters.AddWithValue("@EmailAddress", profile.Email);
                command.Parameters.AddWithValue("@UserId", userId);

                int rowsAffected = command.ExecuteNonQuery();
            }
        }

          return Ok("Profile data inserted successfully.");
    }

    private int GetSupplierIdByApplication(SqlConnection connection, int AppID)
    {
        var query = "SELECT SupplierID FROM Applications WHERE ApplicationID = @ApplicationID";
        return connection.QueryFirstOrDefault<int>(query, new { ApplicationID = AppID });
    }

    private int GetUserIdBySupplierID(SqlConnection connection, int userId)
    {
        var query = "SELECT UserID FROM cfgUser_Supplier_Mapping WHERE SupplierID = @SupplierID";
        return connection.QueryFirstOrDefault<int>(query, new { SupplierID = userId });
    }

}

public class SupplierDocumentDataParameter
{
    public string Field { get; set; }
    public string NewValue { get; set; }
}

public class ProfileModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CompanyName { get; set; }
    public string cellNumber { get; set; }
    public string Category { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string PostalCode { get; set; }
}