using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Mail;
using Dapper;

[ApiController]
[Route("api/[controller]")]
public class InviteController : ControllerBase
{
    private readonly string _connectionString;

    public InviteController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    /*
        [HttpGet("document-classes")]
        public async Task<IActionResult> GetDocumentClasses()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT DocumentClassID, DocumentClass FROM cfgDocumentClasses";
                var command = new SqlCommand(query, connection);

                var documentClasses = new List<DocumentClassDto>();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        documentClasses.Add(new DocumentClassDto
                        {
                            DocumentClassID = reader.GetInt32(0),
                            DocumentClass = reader.GetString(1)
                        });
                    }
                }

                return Ok(documentClasses);
            }
        }
    */

    [HttpGet("document-classes/{supplierTypeId}")]
    public async Task<IActionResult> GetDocumentClassesBySupplierType(int supplierTypeId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Step 1: Get DocumentTypeIDs from mdSupplierDocumentTypes based on SupplierTypeID
            var documentTypeIds = new List<int>();
            var queryDocumentTypes = "SELECT DocumentTypeID FROM mdSupplierDocumentTypes WHERE SupplierTypeID = @SupplierTypeID";

            using (var command = new SqlCommand(queryDocumentTypes, connection))
            {
                command.Parameters.AddWithValue("@SupplierTypeID", supplierTypeId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        documentTypeIds.Add(reader.GetInt32(0));
                    }
                }
            }

            // Step 2: Get DocumentClassIDs from cfgDocumentTypes based on DocumentTypeIDs
            var documentClassIds = new List<int>();
            if (documentTypeIds.Count > 0)
            {
                var queryDocumentClasses = "SELECT DocumentClassID FROM cfgDocumentTypes WHERE DocumentTypeID IN (@DocumentTypeIDs)";

                // Create a parameterized query for multiple IDs
                var parameterList = string.Join(",", documentTypeIds);
                queryDocumentClasses = queryDocumentClasses.Replace("@DocumentTypeIDs", parameterList);

                using (var command = new SqlCommand(queryDocumentClasses, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            documentClassIds.Add(reader.GetInt32(0));
                        }
                    }
                }
            }

            // Step 3: Get DocumentClasses from cfgDocumentClasses based on DocumentClassIDs
            var documentClasses = new List<DocumentClassDto>();
            if (documentClassIds.Count > 0)
            {
                var queryClasses = "SELECT DocumentClassID, DocumentClass FROM cfgDocumentClasses WHERE DocumentClassID IN (@DocumentClassIDs)";

                // Create a parameterized query for multiple IDs
                var classParameterList = string.Join(",", documentClassIds);
                queryClasses = queryClasses.Replace("@DocumentClassIDs", classParameterList);

                using (var command = new SqlCommand(queryClasses, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            documentClasses.Add(new DocumentClassDto
                            {
                                DocumentClassID = reader.GetInt32(0),
                                DocumentClass = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return Ok(documentClasses);
        }
    }

    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes([FromQuery] string documentClassIds)
    {
        if (string.IsNullOrEmpty(documentClassIds))
        {
            return BadRequest("No document class IDs provided.");
        }

        // Split the incoming IDs and parse them into a list of integers
        var ids = documentClassIds.Split(',').Select(int.Parse).ToList();

        // Create a parameterized query for the list of IDs
        var query = "SELECT DocumentTypeID, DocumentType FROM cfgDocumentTypes WHERE DocumentClassID IN (";
        var parameters = new List<SqlParameter>();

        for (int i = 0; i < ids.Count; i++)
        {
            // Add a parameter for each ID
            query += $"@DocumentClassID{i},";
            parameters.Add(new SqlParameter($"@DocumentClassID{i}", ids[i]));
        }

        // Remove the last comma and close the parentheses
        query = query.TrimEnd(',') + ")";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                // Add all parameters to the command
                command.Parameters.AddRange(parameters.ToArray());

                var documentTypes = new List<DocumentTypeDto>();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        documentTypes.Add(new DocumentTypeDto
                        {
                            DocumentTypeID = reader.GetInt32(0),
                            DocumentType = reader.GetString(1)
                        });
                    }
                }

                return Ok(documentTypes);
            }
        }
    }

    [HttpGet("supplier-types")]
    public async Task<IActionResult> GetSupplierTypes()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = "SELECT SupplierTypeID, SupplierTypeName FROM cfgSupplierTypes";

            var command = new SqlCommand(query, connection);

            var supplierTypes = new List<SupplierTypeDto>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    supplierTypes.Add(new SupplierTypeDto
                    {
                        SupplierTypeID = reader.GetInt32(0),
                        SupplierTypeName = reader.GetString(1)
                    });
                }
            }

            return Ok(supplierTypes);

        }
    }

    [HttpGet("question-types")]
    public async Task<IActionResult> GetQuestionTypes()
    {

        var query = "SELECT QuestionTypeID, QuestionType FROM cfgQuestionTypes";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                var questionTypes = new List<QuestionTypeDto>();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        questionTypes.Add(new QuestionTypeDto
                        {
                            QuestionTypeID = reader.GetInt32(0),
                            QuestionType = reader.GetString(1)
                        });
                    }
                }

                return Ok(questionTypes);
            }
        }
    }

    /*
        [HttpGet("questions")]
        public async Task<IActionResult> GetQuestions([FromQuery] string questionTypeIds)
        {
            if (string.IsNullOrEmpty(questionTypeIds))
            {
                return BadRequest("No question type IDs provided.");
            }

            // Split the incoming IDs and parse them into a list of integers
            var ids = questionTypeIds.Split(',').Select(int.Parse).ToList();

            // Create a parameterized query for the list of IDs
            var query = "SELECT QuestionID, QuestionLabel FROM cfgQuestions WHERE QuestionTypeID IN (";
            var parameters = new List<SqlParameter>();

            for (int i = 0; i < ids.Count; i++)
            {
                // Add a parameter for each ID
                query += $"@QuestionTypeID{i},";
                parameters.Add(new SqlParameter($"@QuestionTypeID{i}", ids[i]));
            }

            // Remove the last comma and close the parentheses
            query = query.TrimEnd(',') + ")";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    // Add all parameters to the command
                    command.Parameters.AddRange(parameters.ToArray());

                    var questions = new List<QuestionDto>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            questions.Add(new QuestionDto
                            {
                                QuestionID = reader.GetInt32(0),
                                QuestionLabel = reader.GetString(1)
                            });
                        }
                    }

                    return Ok(questions);
                }
            }
        }
    */

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions([FromQuery] string questionTypeIds, [FromQuery] string documentTypeIds)
    {
        if (string.IsNullOrEmpty(questionTypeIds))
        {
            return BadRequest("No question type IDs provided.");
        }

        // Split the incoming question type IDs and parse them into a list of integers
        var questionIds = questionTypeIds.Split(',').Select(int.Parse).ToList();

        // Create a parameterized query for the list of question type IDs
        var query = "SELECT QuestionID, QuestionLabel FROM cfgQuestions WHERE QuestionTypeID IN (";
        var parameters = new List<SqlParameter>();

        for (int i = 0; i < questionIds.Count; i++)
        {
            // Add a parameter for each question type ID
            query += $"@QuestionTypeID{i},";
            parameters.Add(new SqlParameter($"@QuestionTypeID{i}", questionIds[i]));
        }

        // Remove the last comma and close the parentheses
        query = query.TrimEnd(',') + ")";

        // Add DocumentTypeID filter if provided
        if (!string.IsNullOrEmpty(documentTypeIds))
        {
            var documentIds = documentTypeIds.Split(',').Select(int.Parse).ToList();
            query += " AND DocumentTypeID IN (";

            for (int i = 0; i < documentIds.Count; i++)
            {
                // Add a parameter for each document type ID
                query += $"@DocumentTypeID{i},";
                parameters.Add(new SqlParameter($"@DocumentTypeID{i}", documentIds[i]));
            }

            // Remove the last comma and close the parentheses
            query = query.TrimEnd(',') + ")";
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                // Add all parameters to the command
                command.Parameters.AddRange(parameters.ToArray());

                var questions = new List<QuestionDto>();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        questions.Add(new QuestionDto
                        {
                            QuestionID = reader.GetInt32(0),
                            QuestionLabel = reader.GetString(1)
                        });
                    }
                }

                return Ok(questions);
            }
        }
    }



    [HttpGet("User-Status")]
    public async Task<IActionResult> GetUserStatus([FromQuery] string email, [FromQuery] string applicationid)
    {
        // Check if either email or applicationid is provided
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(applicationid))
        {
            return BadRequest("Either email or application ID must be provided.");
        }

        UserLoginDto user = null;

        // If email is provided, query the cfgUser table
        if (!string.IsNullOrEmpty(email))
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Query to get user details based on email
                var userQuery = await connection.QueryFirstOrDefaultAsync<UserLoginDto>(
                    "SELECT UserID, FirstNames, LastNames, MobileNumber, EmailAddress FROM cfgUser WHERE EmailAddress = @Email",
                    new { Email = email });

                if (userQuery != null)
                {
                    // Get RoleID from cfgUserRoles
                    var roleQuery = await connection.QueryFirstOrDefaultAsync<int>(
                        "SELECT RoleID FROM cfgUserRoles WHERE UserID = @UserID",
                        new { UserID = userQuery.UserID });

                    if (roleQuery == 2)
                    {
                        // Get SupplierID from cfgUser_Supplier_Mapping
                        var supplierQuery = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT SupplierID FROM cfgUser_Supplier_Mapping WHERE UserID = @UserID",
                            new { UserID = userQuery.UserID });

                        // Get ApplicationID from Applications
                        var applicationQuery = await connection.QueryFirstOrDefaultAsync<int>(
                            "SELECT ApplicationID FROM Applications WHERE SupplierID = @SupplierID",
                            new { SupplierID = supplierQuery });

                        // Return the user details along with ApplicationID
                        user = new UserLoginDto
                        {
                            ApplicationID = applicationQuery,
                            FirstNames = userQuery.FirstNames,
                            LastNames = userQuery.LastNames,
                            MobileNumber = userQuery.MobileNumber,
                            EmailAddress = email
                        };
                    }
                    else
                    {
                        // Return user details without ApplicationID
                        user = new UserLoginDto
                        {
                            FirstNames = userQuery.FirstNames,
                            LastNames = userQuery.LastNames,
                            MobileNumber = userQuery.MobileNumber,
                            EmailAddress = email
                        };
                    }
                }
            }
        }

        // If applicationid is provided, you can add additional logic here if needed
        // ...

        return Ok(user);
    }


    //------------------------------------------------------------------------------Supplier Creation- Invite Button------------------------------------------------------------------------// 

    [HttpPost("submit-invite")]
    public async Task<IActionResult> SubmitInvite([FromBody] InviteRequest inviteRequest)
    {
        using (var connection = new SqlConnection(_connectionString))
        {

            await connection.OpenAsync();

            // Save the new Supplier
            string queryUserInfor = "INSERT INTO dbo.mdSuppliers (SupplierName, SupplierTypeID, SupplierAddress, SupplierStatusID, CRUDDateTime) " +
                                     "VALUES (@SupplierName, @SupplierTypeID, @SupplierAddress, @SupplierStatusID, @CRUDDateTime)";

            using (var command = new SqlCommand(queryUserInfor, connection))
            {
                command.Parameters.AddWithValue("@SupplierName", inviteRequest.SupplierName);
                command.Parameters.AddWithValue("@SupplierTypeID", inviteRequest.SupplierTypeID);
                command.Parameters.AddWithValue("@SupplierAddress", inviteRequest.EmailAddress);
                command.Parameters.AddWithValue("@SupplierStatusID", 1); // 1 equals to Invite sent
                command.Parameters.AddWithValue("@CRUDDateTime", DateTime.Now);

                await command.ExecuteNonQueryAsync();
            }

            // Save the User
            string queryUser = "INSERT INTO dbo.cfgUser (UserName, FirstNames, LastNames, MobileNumber, EmailAddress) " +
                               "VALUES (@UserName, @FirstNames, @LastNames, @MobileNumber, @EmailAddress)";

            using (var command = new SqlCommand(queryUser, connection))
            {
                command.Parameters.AddWithValue("@UserName", inviteRequest.UserName);
                command.Parameters.AddWithValue("@FirstNames", inviteRequest.FirstNames);
                command.Parameters.AddWithValue("@LastNames", inviteRequest.LastNames);
                command.Parameters.AddWithValue("@MobileNumber", inviteRequest.MobileNumber);
                command.Parameters.AddWithValue("@EmailAddress", inviteRequest.EmailAddress);

                await command.ExecuteNonQueryAsync();
            }

            // Lookup UserID
            string userLookUp = "SELECT UserID FROM dbo.cfgUser WHERE EmailAddress = @EmailAddress";
            int lookedUpUserID;

            using (var command = new SqlCommand(userLookUp, connection))
            {
                command.Parameters.AddWithValue("@EmailAddress", inviteRequest.EmailAddress);

                object result = await command.ExecuteScalarAsync();
                lookedUpUserID = result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }

            // Insert UserRole
            string insertUserRole = "INSERT INTO dbo.cfgUserRoles (RoleID, UserID) VALUES (@RoleID, @UserID)";

            using (var command = new SqlCommand(insertUserRole, connection))
            {
                command.Parameters.AddWithValue("@RoleID", 2); // Role 2 equals to User/Supplier
                command.Parameters.AddWithValue("@UserID", lookedUpUserID);

                await command.ExecuteNonQueryAsync();
            }

            // Lookup SupplierID and SupplierTypeID
            string supplierIDLookUp = "SELECT SupplierID, SupplierTypeID FROM dbo.mdSuppliers WHERE SupplierAddress = @SupplierAddress";
            int lookedUpSupplierID = 0, lookedUpSupplierTypeID = 0;

            using (var command = new SqlCommand(supplierIDLookUp, connection))
            {
                command.Parameters.AddWithValue("@SupplierAddress", inviteRequest.EmailAddress);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        lookedUpSupplierID = reader.GetInt32(0);
                        lookedUpSupplierTypeID = reader.GetInt32(1);
                    }
                }
            }

            // Insert into UserSupplierMapping
            string insertUserSupplierMapping = "INSERT INTO dbo.cfgUser_Supplier_Mapping (UserID, RoleID, SupplierID) " +
                                               "VALUES (@UserID, @RoleID, @SupplierID)";

            using (var command = new SqlCommand(insertUserSupplierMapping, connection))
            {
                command.Parameters.AddWithValue("@RoleID", 2); // Role 2 equals to User/Supplier
                command.Parameters.AddWithValue("@UserID", lookedUpUserID);
                command.Parameters.AddWithValue("@SupplierID", lookedUpSupplierID);

                await command.ExecuteNonQueryAsync();
            }

            // Insert into mdSupplierDocumentTypes
            foreach (var documentTypeIDSY in inviteRequest.DocumentTypeIDs)
            {
                string querySupplierDocType = "INSERT INTO dbo.mdSupplierDocumentTypes (DocumentTypeID, SupplierTypeID) " +
                                               "VALUES (@DocumentTypeID, @SupplierTypeID)";

                using (var command = new SqlCommand(querySupplierDocType, connection))
                {
                    command.Parameters.AddWithValue("@DocumentTypeID", documentTypeIDSY);
                    command.Parameters.AddWithValue("@SupplierTypeID", lookedUpSupplierTypeID);

                    await command.ExecuteNonQueryAsync();
                }
            }

            // Create application - ApplicationID 
            string querySupplierApplicationID = "INSERT INTO dbo.Applications (SupplierID, ApplicationStatusID) " +
                                                 "VALUES (@SupplierID, @ApplicationStatusID); " +
                                                 "SELECT SCOPE_IDENTITY();"; 

            int newApplicationID; 

            using (var command = new SqlCommand(querySupplierApplicationID, connection))
            {
                command.Parameters.AddWithValue("@SupplierID", lookedUpSupplierID);
                command.Parameters.AddWithValue("@ApplicationStatusID", 1);

                // Execute the command and retrieve the ApplicationID
                var applicationId = await command.ExecuteScalarAsync(); 
                newApplicationID = Convert.ToInt32(applicationId);  
            }


            //insert the infor inside the applicationData
            string queryApplicationDataMap = "INSERT INTO dbo.ApplicationData (DocumentTypeID, ApplicationID, QuestionID) " +
                                        "VALUES (@DocumentTypeID, @ApplicationID, @QuestionID)";


            foreach (var documentTypeID in inviteRequest.DocumentTypeIDs)
            {
                foreach (var questionID in inviteRequest.QuestionIDs)
                {
                    using (var command = new SqlCommand(queryApplicationDataMap, connection))
                    {
                        command.Parameters.AddWithValue("@DocumentTypeID", documentTypeID);
                        command.Parameters.AddWithValue("@ApplicationID", newApplicationID);
                        command.Parameters.AddWithValue("@QuestionID", questionID);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }

            // Send the email
            await SendInvitationEmail(inviteRequest.EmailAddress, inviteRequest.SupplierName, newApplicationID);

            return Ok("Invite submitted successfully");
        }
    }

    // Method to send the invitation email
    private async Task SendInvitationEmail(string emailAddress, string supplierName, int applicationId)
    {
        string registrationUrl = $"http://localhost:3000/register?applicationId={applicationId}";

        // Construct the email
        MailMessage mail = new MailMessage("vendorportalmgt@gmail.com", emailAddress)
        {
            Subject = "You're Invited to Register on Our Vendor Platform!",
            IsBodyHtml = true,
            Body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f4f4;
                    color: #333;
                }}
                .container {{
                    background-color: white;
                    padding: 20px;
                    max-width: 600px;
                    margin: 0 auto;
                    border-radius: 8px;
                    box-shadow: 0 0 10px rgba(0,0,0,0.1);
                }}
                h1 {{
                    color: #0056b3;
                }}
                p {{
                    font-size: 16px;
                    line-height: 1.5;
                }}
                .btn {{
                    display: inline-block;
                    background-color: #0056b3;
                    color: white;
                    padding: 10px 20px;
                    text-decoration: none;
                    border-radius: 5px;
                    margin-top: 20px;
                }}
                .btn:hover {{
                    background-color: #003d80;
                }}
                .footer {{
                    margin-top: 40px;
                    font-size: 12px;
                    color: #777;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <img src='http://firtech.co.za/wp-content/uploads/2020/08/original-website-logo1.png' alt='Welcome' style='width:20%; border-radius: 8px 8px 0 0;'>
                <h1>You're Invited to Join Our Vendor Platform!</h1>
                <p>Hello <strong>{supplierName}</strong>,</p>
                <p>We are excited to invite you to register on our exclusive vendor platform, where you can streamline your processes, submit invoices, and collaborate with us effortlessly.</p>
                <p>To get started, please click the button below and complete your registration.</p>
                <a href='{registrationUrl}' class='btn'>Register Now</a>
                <p>If you have any questions, feel free to reach out to our support team.</p>
                <p>Looking forward to working with you!</p>
                <div class='footer'>
                    <p>&copy; 2025 FIRtech Intelligence Automation. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>"
        };

        // Send the email
        using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
        {
            // Use secure methods to store credentials
            smtpClient.Credentials = new System.Net.NetworkCredential("vendorportalmgt@gmail.com", "ynzcncktxbxgpbjq"); // Replace with your actual password or use an App Password
            smtpClient.EnableSsl = true; // Ensure SSL is enabled

            try
            {
                await smtpClient.SendMailAsync(mail);
            }
            catch (SmtpException smtpEx)
            {
                // Log the SMTP exception
                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                throw; // Rethrow or handle as needed
            }
            catch (Exception ex)
            {
                // Log any other exceptions
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw; // Rethrow or handle as needed
            }
        }
    }

}


public class UserLoginDto
{
    public int UserID { get; set; }
    public int ApplicationID { get; set; }
    public string FirstNames { get; set; }
    public string LastNames { get; set; }
    public string MobileNumber { get; set; }
    public string EmailAddress { get; set; }
}

public class DocumentClassDto
{
    public int DocumentClassID { get; set; }
    public string DocumentClass { get; set; }
}

public class DocumentTypeDto
{
    public int DocumentTypeID { get; set; }
    public string DocumentType { get; set; }
}

public class SupplierTypeDto
{
    public int SupplierTypeID { get; set; }
    public string SupplierTypeName { get; set; }
}

public class QuestionTypeDto
{
    public int QuestionTypeID { get; set; }
    public string QuestionType { get; set; }
}

public class QuestionDto
{
    public int QuestionID { get; set; }
    public string QuestionLabel { get; set; }
}

public class InviteRequest
{
    public string SupplierName { get; set; }
    public int SupplierTypeID { get; set; }
    public string SupplierAddress { get; set; }
    public string UserName { get; set; }
    public string FirstNames { get; set; }
    public string LastNames { get; set; }
    public string MobileNumber { get; set; }
    public string EmailAddress { get; set; }
    public List<int> DocumentTypeIDs { get; set; }
    public List<int> DocumentClassIDs { get; set; }
    public List<int> QuestionIDs { get; set; }
}