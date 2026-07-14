using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.Document;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AI.TestCaseGenerator.API.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public DocumentService(
            ApplicationDbContext context,
            IMapper mapper,
            IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

       

        public async Task<ProcessDocumentResponseDto> ProcessDocumentAsync(int documentId)
{
    try
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(x => x.Id == documentId);

        if (document == null)
            return new ProcessDocumentResponseDto
            {
                Success = false,
                Message = "Document not found."
            };

        if (!File.Exists(document.FilePath))
            return new ProcessDocumentResponseDto
            {
                Success = false,
                Message = "Document file is missing on disk."
            };

        string extractedText = await ExtractTextAsync(document);

        if (string.IsNullOrWhiteSpace(extractedText))
            return new ProcessDocumentResponseDto
            {
                Success = false,
                Message = "No text could be extracted from the document."
            };

        List<string> chunks = ChunkText(extractedText);

        await SaveChunksAsync(document.Id, chunks);

        await GenerateEmbeddingsAsync(document.Id);

        return new ProcessDocumentResponseDto
        {
            Success = true,
            Message = "Document processed successfully.",
            TotalChunks = chunks.Count
        };
    }
    catch (Exception ex)
    {
        return new ProcessDocumentResponseDto
        {
            Success = false,
            Message = ex.Message
        };
    }
}


private async Task<string> ExtractTextAsync(Document document)
{
    string extension = Path.GetExtension(document.FilePath).ToLowerInvariant();

    if (extension == ".txt")
    {
        return await File.ReadAllTextAsync(document.FilePath);
    }

    if (extension == ".pdf")
        return await ExtractPdfTextAsync(document.FilePath);

    if (extension == ".docx")
        return await ExtractDocxTextAsync(document.FilePath);

    return await Task.FromResult(string.Empty);
}

        private async Task<string> ExtractDocxTextAsync(string filePath)
        {
            return await Task.FromResult("DOCX extraction is not implemented yet.");
        }

        private async Task<string> ExtractPdfTextAsync(string filePath)
        {
            return await Task.FromResult("PDF extraction is not implemented yet.");
        }

        private List<string> ChunkText(string text)
{
    const int chunkSize = 1000;

    List<string> chunks = new();

    for (int i = 0; i < text.Length; i += chunkSize)
    {
        chunks.Add(text.Substring(
            i,
            Math.Min(chunkSize, text.Length - i)));
    }

    return chunks;
}

private async Task SaveChunksAsync(
    int documentId,
    List<string> chunks)
{
    int index = 1;

    foreach (var chunk in chunks)
    {
        _context.DocumentChunks.Add(new DocumentChunk
        {
            DocumentId = documentId,
            ChunkIndex = index++,
            Content = chunk
        });
    }

    await _context.SaveChangesAsync();
}

private async Task GenerateEmbeddingsAsync(int documentId)
{
    await Task.CompletedTask;

    // Claude API + ChromaDB implementation
    // will be added later.
}



        public async Task<DocumentResponseDto> UploadDocumentAsync(
    UploadDocumentDto dto,
    IFormFile file,
    int userId)
{
    // Validate Project
    var project = await _context.Projects
        .FirstOrDefaultAsync(p =>
            p.Id == dto.ProjectId &&
            p.UserId == userId);

    if (project == null)
        throw new Exception("Project not found.");

    // Upload folder
    var uploadFolder = _configuration["FileStorage:UploadPath"]!;

    if (!Directory.Exists(uploadFolder))
        Directory.CreateDirectory(uploadFolder);

    // Generate unique filename
    var uniqueFileName =
        $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

    var filePath = Path.Combine(uploadFolder, uniqueFileName);

    // Save file
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    // Save metadata
    var document = new Document
    {
        FileName = file.FileName,
        FileType = Path.GetExtension(file.FileName),
        FilePath = filePath,
        FileSize = file.Length,
        ProjectId = dto.ProjectId
    };

    _context.Documents.Add(document);

    await _context.SaveChangesAsync();

    return _mapper.Map<DocumentResponseDto>(document);
}

public async Task<IEnumerable<DocumentResponseDto>> GetProjectDocumentsAsync(
    int projectId,
    int userId)
{
    var projectExists = await _context.Projects
        .AnyAsync(p =>
            p.Id == projectId &&
            p.UserId == userId);

    if (!projectExists)
        return Enumerable.Empty<DocumentResponseDto>();

    var documents = await _context.Documents
        .Where(d => d.ProjectId == projectId)
        .OrderByDescending(d => d.CreatedAt)
        .ToListAsync();

    return _mapper.Map<IEnumerable<DocumentResponseDto>>(documents);
}

public async Task<DocumentResponseDto?> GetDocumentByIdAsync(int documentId, int userId)
{
    var document = await _context.Documents
        .Include(d => d.Project)
        .FirstOrDefaultAsync(d =>
            d.Id == documentId &&
            d.Project.UserId == userId);

    if (document == null)
        return null;

    return _mapper.Map<DocumentResponseDto>(document);
}

public async Task<DocumentDownloadDto?> DownloadDocumentAsync(
    int documentId,
    int userId)
{
    var document = await _context.Documents
        .Include(d => d.Project)
        .FirstOrDefaultAsync(d =>
            d.Id == documentId &&
            d.Project.UserId == userId);

    if (document == null || !File.Exists(document.FilePath))
        return null;

    var bytes = await File.ReadAllBytesAsync(document.FilePath);
    var contentType = document.FileType.ToLowerInvariant() switch
    {
        ".txt" => "text/plain",
        ".pdf" => "application/pdf",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };

    return new DocumentDownloadDto
    {
        FileBytes = bytes,
        FileName = document.FileName,
        FilePath = document.FilePath,
        FileType = document.FileType,
        ContentType = contentType,
        FileSize = bytes.Length
    };
}

public async Task<bool> DeleteDocumentAsync(
    int documentId,
    int userId)
{
    var document = await _context.Documents
        .Include(d => d.Project)
        .FirstOrDefaultAsync(d =>
            d.Id == documentId &&
            d.Project.UserId == userId);

    if (document == null)
        return false;

    if (File.Exists(document.FilePath))
        File.Delete(document.FilePath);

    _context.Documents.Remove(document);

    await _context.SaveChangesAsync();

    return true;
}

    }
}