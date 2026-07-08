using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace AI.TestCaseGenerator.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ExportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExportController> _logger;

        public ExportController(
            ApplicationDbContext context,
            ILogger<ExportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Export all test cases of a project to Excel.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        [HttpGet("excel/{projectId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportToExcel(int projectId)
        {
            var userId = GetCurrentUserId();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
                return NotFound(new { Success = false, Message = "Project not found." });

            var testCases = await _context.TestCases
                .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                .OrderBy(t => t.Id)
                .ToListAsync();

            var csv = BuildCsv(testCases);
            var bytes = Encoding.UTF8.GetBytes(csv);
            var fileName = $"{project.Name.Replace(" ", "_")}_test_cases.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        /// <summary>
        /// Export all test cases of a project to PDF.
        /// </summary>
        [HttpGet("pdf/{projectId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportToPdf(int projectId)
        {
            var userId = GetCurrentUserId();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
                return NotFound(new { Success = false, Message = "Project not found." });

            var testCases = await _context.TestCases
                .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                .OrderBy(t => t.Id)
                .ToListAsync();

            var pdfBytes = BuildPdf(project.Name, testCases);
            return File(pdfBytes, "application/pdf", $"{project.Name.Replace(" ", "_")}_test_cases.pdf");
        }

        /// <summary>
        /// Export a single test case to PDF.
        /// </summary>
        [HttpGet("pdf/testcase/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportSingleTestCase(int id)
        {
            var userId = GetCurrentUserId();

            var testCase = await _context.TestCases
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project!.UserId == userId && !t.IsDeleted);

            if (testCase == null)
                return NotFound(new { Success = false, Message = "Test case not found." });

            var pdfBytes = BuildPdf(testCase.Title, new List<TestCase> { testCase });
            return File(pdfBytes, "application/pdf", $"{testCase.Title.Replace(" ", "_")}.pdf");
        }

        private static string BuildCsv(List<TestCase> testCases)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Title,Type,Priority,Preconditions,Steps,Expected Result");

            foreach (var testCase in testCases)
            {
                sb.AppendLine($"\"{EscapeCsv(testCase.Title)}\",\"{EscapeCsv(testCase.TestType)}\",\"{EscapeCsv(testCase.Priority)}\",\"{EscapeCsv(testCase.Preconditions)}\",\"{EscapeCsv(testCase.TestSteps)}\",\"{EscapeCsv(testCase.ExpectedResult)}\"");
            }

            return sb.ToString();
        }

        private static string EscapeCsv(string? value) => (value ?? string.Empty).Replace("\"", "\"\"");

        private static byte[] BuildPdf(string title, List<TestCase> testCases)
        {
            var sb = new StringBuilder();
            sb.AppendLine("%PDF-1.4");
            sb.AppendLine("1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj");
            sb.AppendLine("2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj");
            sb.AppendLine("3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>endobj");
            sb.AppendLine("4 0 obj<< /Length 0 >>stream\nendstream\nendobj");
            sb.AppendLine("5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj");

            var content = $"BT /F1 12 Tf 50 750 Td ({EscapePdf(title)}) Tj 0 -20 Td ({EscapePdf($"Exported {testCases.Count} test case(s)")}) Tj";
            foreach (var testCase in testCases)
            {
                content += $" 0 -25 Td ({EscapePdf(testCase.Title)}) Tj";
            }

            content += " ET";

            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);
            writer.WriteLine("%PDF-1.4");
            writer.WriteLine("1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj");
            writer.WriteLine("2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj");
            writer.WriteLine("3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>endobj");
            writer.WriteLine("4 0 obj<< /Length 0 >>stream");
            writer.WriteLine(content);
            writer.WriteLine("endstream");
            writer.WriteLine("endobj");
            writer.WriteLine("5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj");
            writer.Flush();
            return stream.ToArray();
        }

        private static string EscapePdf(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            return int.Parse(claim.Value);
        }
    }
}