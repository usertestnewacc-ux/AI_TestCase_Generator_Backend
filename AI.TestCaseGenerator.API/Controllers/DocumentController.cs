using AI.TestCaseGenerator.API.DTOs.Document;
using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI.TestCaseGenerator.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(
            IDocumentService documentService,
            ILogger<DocumentController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a document (PDF, DOCX, TXT).
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadDocument(
            [FromForm] UploadDocumentDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("Please select a file.");

            try
            {
                int userId = GetCurrentUserId();

                var document = await _documentService.UploadDocumentAsync(dto, dto.File, userId);

                return CreatedAtAction(
                    nameof(GetDocumentById),
                    new { id = document.Id },
                    document);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all documents of a project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        [ProducesResponseType(typeof(IEnumerable<DocumentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProjectDocuments(int projectId)
        {
            try
            {
                int userId = GetCurrentUserId();

                var documents = await _documentService.GetProjectDocumentsAsync(projectId, userId);

                return Ok(documents);
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get document details.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var document = await _documentService.GetDocumentByIdAsync(id, userId);

                if (document == null)
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Document not found."
                    });

                return Ok(document);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Download uploaded document.
        /// </summary>
        [HttpGet("download/{id:int}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                int userId = GetCurrentUserId();

                var file = await _documentService.DownloadDocumentAsync(id, userId);

                if (file == null)
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Document not found."
                    });

                return File(
                    file.FileBytes,
                    file.ContentType,
                    file.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete document.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                int userId = GetCurrentUserId();

                bool deleted = await _documentService.DeleteDocumentAsync(id, userId);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Document not found."
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Document deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Process uploaded document.
        /// Extract text, split into chunks and generate embeddings.
        /// </summary>
        [HttpPost("process/{id:int}")]
        public async Task<IActionResult> ProcessDocument(int id)
        {
            try
            {
                var result = await _documentService.ProcessDocumentAsync(id);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            return int.Parse(claim.Value);
        }
    }
}