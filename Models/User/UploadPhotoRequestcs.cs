// File: Dtos/UploadPhotoRequest.cs
using Microsoft.AspNetCore.Http;

namespace backtimetracker.Dtos
{
    public class UploadPhotoRequest
    {
        // نام این property دقیقا باید با کلید فرم یکی باشد (مثلا "file")
        public IFormFile file { get; set; }
    }
}
