using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.BLL.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder);
    void DeleteImage(string filePath);
    string[] GetAllowedExtensions();
    long GetMaxFileSize();
}
