using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Threading.Tasks;


    [ApiController]
    [Route("api/[controller]")]
    public class loginController : ControllerBase
    {
        private readonly string _connectionString;
        public loginController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("GetDocumentTypes/{applicationId}")]
        public async Task<IActionResult> GetDocumentTypes(int applicationId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                // Check if ApplicationID exists
                var applicationExists = await connection.QuerySingleOrDefaultAsync<int>(
                    "SELECT COUNT(1) FROM ApplicationData WHERE ApplicationID = @ApplicationID",
                    new { ApplicationID = applicationId });

                if (applicationExists == 0)
                {
                    return NotFound(new { Message = "ApplicationID does not exist." });
                }

                // Fetch DocumentTypeIDs
                var documentTypeIds = await connection.QueryAsync<int>(
                    "SELECT DocumentTypeID FROM ApplicationData WHERE ApplicationID = @ApplicationID",
                    new { ApplicationID = applicationId });

                if (!documentTypeIds.Any())
                {
                    return NotFound(new { Message = "No document types found for this ApplicationID." });
                }

                // Fetch DocumentType details
                var documentTypes = await connection.QueryAsync(
                    @"SELECT DocumentTypeID, DocumentType, DocumentTypeDescription
              FROM cfgDocumentTypes
              WHERE DocumentTypeID IN @DocumentTypeIDs",
                    new { DocumentTypeIDs = documentTypeIds });

                return Ok(documentTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

    }

