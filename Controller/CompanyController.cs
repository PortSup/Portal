using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly string _connectionString;

    public CompanyController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }


    [HttpPost("InsertCompanyInformation")]
    public IActionResult InsertCompanyInformation([FromBody] CompanyInforModel company)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Step 1: Get SupplierID and DocumentTypeID using the email
                int? userId = GetUserIdByEmail(connection, "macD@co.za");
                if (userId == null) return NotFound("User not found.");

                int? supplierId = GetSupplierIdByUserId(connection, userId.Value);
                if (supplierId == null) return NotFound("Supplier not found.");

                int? applicationId = GetApplicationIdBySupplierId(connection, supplierId.Value);
                if (applicationId == null) return NotFound("Application not found.");

                int? documentTypeId = GetDocumentTypeIdByApplicationId(connection, applicationId.Value);
                if (documentTypeId == null) return NotFound("Document Type not found.");

                // Step 2: Insert profile data into SupplierDocumentData
                var insertQuery = "INSERT INTO [dbo].[SupplierDocumentData] (Field, NewValue, SupplierID, DocumentTypeID) VALUES (@Field, @NewValue, @SupplierID, @DocumentTypeID)";

                // Prepare parameters for insertion
                var parameters = new List<SupplierDocumentParameter>
            {
                new SupplierDocumentParameter { Field = "StartDate", NewValue = company.StartDate, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierDocumentParameter { Field = "CompanyType", NewValue = company.CompanyType, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierDocumentParameter { Field = "RegistrationNumber", NewValue = company.RegistrationNumber, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierDocumentParameter { Field = "RegistrationDate", NewValue = company.RegistrationDate, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierDocumentParameter { Field = "Address", NewValue = company.Address, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierDocumentParameter { Field = "CertificateIssuer", NewValue = company.CertificateIssuer, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierDocumentParameter { Field = "TaxNumber", NewValue = company.TaxNumber, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value }
            };

                foreach (var param in parameters)
                {
                    connection.Execute(insertQuery, param);
                }
            }

            return Ok("Profile data inserted successfully.");
        }
        catch (Exception ex)
        {
            // Log the exception (consider using a logging framework)
            Console.WriteLine(ex.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    private int? GetUserIdByEmail(SqlConnection connection, string email)
    {
        var query = "SELECT UserID FROM cfgUser WHERE EmailAddress = @EmailAddress";
        return connection.QueryFirstOrDefault<int?>(query, new { EmailAddress = email });
    }

    private int? GetSupplierIdByUserId(SqlConnection connection, int userId)
    {
        var query = "SELECT SupplierID FROM cfgUser_Supplier_Mapping WHERE UserID = @UserID";
        return connection.QueryFirstOrDefault<int?>(query, new { UserID = userId });
    }

    private int? GetApplicationIdBySupplierId(SqlConnection connection, int supplierId)
    {
        var query = "SELECT ApplicationID FROM Applications WHERE SupplierID = @SupplierID";
        return connection.QueryFirstOrDefault<int?>(query, new { SupplierID = supplierId });
    }

    private int? GetDocumentTypeIdByApplicationId(SqlConnection connection, int applicationId)
    {
        var query = "SELECT DocumentTypeID FROM ApplicationData WHERE ApplicationID = @ApplicationID";
        return connection.QueryFirstOrDefault<int?>(query, new { ApplicationID = applicationId });
    }
}

public class SupplierDocumentParameter
{
    public string Field { get; set; }
    public string NewValue { get; set; }
    public int SupplierID { get; set; }
    public int DocumentTypeID { get; set; }

}

public class CompanyInforModel
{
    public string StartDate { get; set; }
    public string CompanyType { get; set; }
    public string RegistrationNumber { get; set; }
    public string Address { get; set; }
    public string RegistrationDate { get; set; }
    public string CertificateIssuer { get; set; }
    public string TaxNumber { get; set; }
}