using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace web_api_users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public ImageController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        [Route("UpLoadImage")]
        public async Task<IActionResult> UpLoadImage(IFormFile file)
        {
            try
            {
                //var file = Request.Form.Files[0];
                string fName = file.FileName;
                string path = Path.Combine(_environment.ContentRootPath, "Images", file.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return Ok($"{file.FileName} successfully uploaded to the Server");
            }
            catch (Exception ex)
            {
                return Conflict("Error al guardar la imagen" + ex);
            }
        }

        [HttpPost()]
        [Route("GetImage")]
        public async Task<IActionResult> GetImage(string imageName)
        {
            try
            {
                Byte[] b;
                b = await System.IO.File.ReadAllBytesAsync(Path.Combine(_environment.ContentRootPath, "Images", $"{imageName}.jpg"));
                return File(b, "image/jpeg");
            }
            catch (Exception ex)
            {
                return Conflict("Error al enviar la imagen" + ex);
            }            
        }
    }
}
