using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;

[ApiController]
[Route("api/[controller]")]
public class BankAccountController : ControllerBase
{
    private readonly string _connectionString;

    public BankAccountController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpPost("InsertBankConfirmation")]
  public IActionResult InsertBankConfirmation([FromBody] BankAccountModel bank)
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
                var parameters = new List<SupplierBankDocumentParameter>
            {
                new SupplierBankDocumentParameter { Field = "BankName", NewValue = bank.BankName, OriginalValue = bank.BankName,  SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierBankDocumentParameter { Field = "BankBranchName", NewValue = bank.BankBranchName, OriginalValue = bank.BankBranchName, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierBankDocumentParameter { Field = "BankBranchCode", NewValue = bank.BankBranchCode, OriginalValue = bank.BankBranchCode, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierBankDocumentParameter { Field = "AccountNumber", NewValue = bank.AccountNumber, OriginalValue = bank.AccountNumber, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierBankDocumentParameter { Field = "AccountType", NewValue = bank.AccountType, OriginalValue = bank.AccountType, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value },
                new SupplierBankDocumentParameter { Field = "BankAccountHolderName", NewValue = bank.BankAccountHolderName, OriginalValue = bank.BankAccountHolderName, SupplierID = supplierId.Value, DocumentTypeID = documentTypeId.Value }
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


public class SupplierBankDocumentParameter
{
    public string Field { get; set; }
    public string NewValue { get; set; }
    public string OriginalValue { get; set; }
    public int SupplierID { get; set; }
    public int DocumentTypeID { get; set; }

}

public class BankAccountModel
{

    public string BankName { get; set; }
    public string BankBranchName { get; set; }
    public string BankBranchCode { get; set; }
    public string AccountNumber { get; set; }
    public string AccountType { get; set; }
    public string BankAccountHolderName { get; set; }
}