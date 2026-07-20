using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.Export;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AI.TestCaseGenerator.API.Services
{
    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<ExportFileDto> ExportProjectTestCasesToExcelAsync(int projectId, int userId)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId);
            if (!projectExists)
                throw new UnauthorizedAccessException("Project not found for current user.");

            var testCases = await _context.TestCases
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.Id)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Test Cases");

            worksheet.Cell(1, 1).Value = "Title";
            worksheet.Cell(1, 2).Value = "Module";
            worksheet.Cell(1, 3).Value = "Type";
            worksheet.Cell(1, 4).Value = "Priority";
            worksheet.Cell(1, 5).Value = "Preconditions";
            worksheet.Cell(1, 6).Value = "Steps";
            worksheet.Cell(1, 7).Value = "Expected Result";
            worksheet.Cell(1, 8).Value = "Description";

            for (int i = 0; i < testCases.Count; i++)
            {
                var row = i + 2;
                var tc = testCases[i];
                worksheet.Cell(row, 1).Value = tc.Title;
                worksheet.Cell(row, 2).Value = tc.ModuleName;
                worksheet.Cell(row, 3).Value = tc.TestType;
                worksheet.Cell(row, 4).Value = tc.Priority;
                worksheet.Cell(row, 5).Value = tc.Preconditions;
                worksheet.Cell(row, 6).Value = tc.TestSteps;
                worksheet.Cell(row, 7).Value = tc.ExpectedResult;
                worksheet.Cell(row, 8).Value = tc.Description;
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ExportFileDto
            {
                FileBytes = stream.ToArray(),
                FileName = $"testcases-project-{projectId}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        public async Task<ExportFileDto> ExportProjectTestCasesToPdfAsync(int projectId, int userId)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId);
            if (!projectExists)
                throw new UnauthorizedAccessException("Project not found for current user.");

            var testCases = await _context.TestCases
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.Id)
                .ToListAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.Header().Text($"Test Cases for Project {projectId}").SemiBold().FontSize(20);
                    page.Content().Column(column =>
                    {
                        foreach (var tc in testCases)
                        {
                            column.Item().PaddingBottom(12).Border(1).Padding(8).Column(inner =>
                            {
                                inner.Item().Text(tc.Title).SemiBold().FontSize(14);
                                inner.Item().Text($"Module: {tc.ModuleName}");
                                inner.Item().Text($"Type: {tc.TestType}");
                                inner.Item().Text($"Priority: {tc.Priority}");
                                inner.Item().Text($"Preconditions: {tc.Preconditions}");
                                inner.Item().Text($"Steps: {tc.TestSteps}");
                                inner.Item().Text($"Expected Result: {tc.ExpectedResult}");
                            });
                        }
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);

            return new ExportFileDto
            {
                FileBytes = stream.ToArray(),
                FileName = $"testcases-project-{projectId}.pdf",
                ContentType = "application/pdf"
            };
        }
    }
}
