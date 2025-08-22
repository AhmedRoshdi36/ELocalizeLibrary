using LibraryManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.BLL.Services;

public class ImageService : IImageService
{
    public async Task<string> SaveImageAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            return null;

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadDir, fileName);

        // Ensure the file stream is closed after writing
        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream);
        }

        return "/" + folder.Replace("\\", "/") + "/" + fileName;
    }

    public void DeleteImage(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

        if (File.Exists(physicalPath))
        {
            try
            {
                // Release handle if locked
                GC.Collect();
                GC.WaitForPendingFinalizers();

                File.Delete(physicalPath);
            }
            catch (IOException ex)
            {
                // Log instead of crashing
                Console.WriteLine($"⚠️ Could not delete image: {ex.Message}");
            }
        }
    }
}
