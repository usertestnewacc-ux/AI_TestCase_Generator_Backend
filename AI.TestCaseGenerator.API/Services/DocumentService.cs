using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.Document;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
    var document = await _context.Documents
        .FirstOrDefaultAsync(x => x.Id == documentId);

    if (document == null)
        throw new Exception("Document not found.");

    // Step 1
    string extractedText = await ExtractTextAsync(document);

    // Step 2
    List<string> chunks = ChunkText(extractedText);

    // Step 3
    await SaveChunksAsync(document.Id, chunks);

    // Step 4
    await GenerateEmbeddingsAsync(document.Id);

    return new ProcessDocumentResponseDto
    {
        Success = true,
        Message = "Document processed successfully.",
        TotalChunks = chunks.Count
    };
}


private async Task<string> ExtractTextAsync(Document document)
{
    string extension = Path.GetExtension(document.FilePath).ToLower();

    if (extension == ".pdf")
        return await ExtractPdfTextAsync(document.FilePath);

    if (extension == ".docx")
        return await ExtractDocxTextAsync(document.FilePath);

    throw new Exception("Unsupported document type.");
}

        private async Task<string> ExtractDocxTextAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        private async Task<string> ExtractPdfTextAsync(string filePath)
        {
            throw new NotImplementedException();
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
        throw new Exception("Project not found.");

    var documents = await _context.Documents
        .Where(d => d.ProjectId == projectId)
        .OrderByDescending(d => d.CreatedAt)
        .ToListAsync();

    return _mapper.Map<IEnumerable<DocumentResponseDto>>(documents);
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

    if (document == null)
        return null;

    return new DocumentDownloadDto
    {
        FileName = document.FileName,
        FilePath = document.FilePath,
        FileType = document.FileType
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