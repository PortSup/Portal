using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;



[ApiController]
[Route("api/[controller]")]
public class TaxController : ControllerBase
{
    private readonly string _connectionString;

    public TaxController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }


    [HttpPost("InsertTaxCompliance")]
    public IActionResult InsertTaxCompliance([FromBody] TaxModel tax)
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
                var insertQuery = "INSERT INTO [dbo].[SupplierDocumentData] (Field, NewValue, OriginalValue, SupplierID, DocumentTypeID) VALUES (@Field, @NewValue, @OriginalValue, @SupplierID, @DocumentTypeID)";

                // Prepare parameters for insertion
                var parameters = new List<SupplierTaxDocumentParameter>
            {
                new SupplierTaxDocumentParameter { Field = "CompanyName", NewValue = tax.CompanyName, OriginalValue = tax.CompanyName, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierTaxDocumentParameter { Field = "IncomeTax", NewValue = tax.IncomeTax, OriginalValue = tax.IncomeTax , SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierTaxDocumentParameter { Field = "ValueAddedTax", NewValue = tax.ValueAddedTax, OriginalValue = tax.ValueAddedTax,  SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierTaxDocumentParameter { Field = "TaxPayerRefNum", NewValue = tax.TaxPayerRefNum, OriginalValue = tax.TaxPayerRefNum,  SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierTaxDocumentParameter { Field = "PAYE", NewValue = tax.PAYE, OriginalValue = tax.PAYE, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierTaxDocumentParameter { Field = "TaxPayerName", NewValue = tax.TaxPayerName, OriginalValue = tax.TaxPayerName,  SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierTaxDocumentParameter { Field = "PIN", NewValue = tax.PIN, OriginalValue = tax.PIN,  SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value }
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


public class SupplierTaxDocumentParameter
{
    public string Field { get; set; }
    public string NewValue { get; set; }
    public string OriginalValue { get; set; }
    public int SupplierID { get; set; }
    public int DocumentTypeID { get; set; }

}


public class TaxModel
{
    public string CompanyName { get; set; }
    public string IncomeTax { get; set; }
    public string ValueAddedTax { get; set; }
    public string TaxPayerRefNum { get; set; }
    public string PAYE { get; set; }
    public string TaxPayerName { get; set; }
    public string PIN { get; set; }

}