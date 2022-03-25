using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using System;
using System.IO;
using System.Threading.Tasks;
using web_api_users.Controllers.Clients;
using web_api_users.Controllers.ParamsDTO;

namespace web_api_users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MinioController : ControllerBase
    {
        // private MinioClient _minio;
        private readonly IFileManagerFactory _fileManagerFactory;
        private readonly IWebHostEnvironment _environment;

        public MinioController(IFileManagerFactory fileManagerFactory, IWebHostEnvironment environment)
        {
            _fileManagerFactory = fileManagerFactory;
            _environment = environment;
        }

        [HttpPost]
        [Route("ConnectToMINio")]
        public async Task<IActionResult> ConnectToMINio([FromBody] CredentialsMINio credentials)
        /*
            {
                "endpoint": "192.168.1.17:9000",
                "accessKey": "admin",
                "secretKey": "password"
            }
         */
        {
            try
            {
                _fileManagerFactory.SetupMinio(new MinioClient()
                                    .WithEndpoint(credentials.endpoint)
                                    .WithCredentials(credentials.accessKey,
                                             credentials.secretKey)
                                    //.WithSSL()
                                    .Build());

                var listBucketResponse = await _fileManagerFactory.GetMinio().ListBucketsAsync();

                var lista = "";

                foreach (var bucket in listBucketResponse.Buckets)
                {
                    lista += "\n bucket '" + bucket.Name + "' created at " + bucket.CreationDate + "\n";
                }

                //var lista = listarBuckets(_fileManagerFactory.GetMinio());

                return Ok("Conecto al MYMINIO: " + lista);
            }
            catch (Exception ex)
            {
                return Conflict("No Conecto al MYMINIO" + ex);
            }
        }

        //private async Task<string> listarBuckets(MinioClient minio)
        //{
        //    var listBucketResponse = await minio.ListBucketsAsync();

        //    var lista = "";

        //    foreach (var bucket in listBucketResponse.Buckets)
        //    {
        //        lista += "\n bucket '" + bucket.Name + "' created at " + bucket.CreationDate + "\n";
        //    }

        //    return lista;
        //}

        [HttpPost]
        [Route("CreateBucketMINio")]
        public async Task<IActionResult> CreateBucketMINio(string name)
        {
            try
            {
                bool found = await _fileManagerFactory.GetMinio().BucketExistsAsync(name);
                if (!found)
                {
                    await _fileManagerFactory.GetMinio().MakeBucketAsync(name, "D:\\Data");
                }
                else
                {
                    return Conflict("No se creo el bucket, YA EXISTE " + name);
                }

                var listBucketResponse = await _fileManagerFactory.GetMinio().ListBucketsAsync();

                var lista = "";

                foreach (var bucket in listBucketResponse.Buckets)
                {
                    lista += "\n bucket '" + bucket.Name + "' created at " + bucket.CreationDate + "\n";
                }

                return Ok("Se creo el bucket: " + lista);
            }
            catch (Exception ex)
            {
                return Conflict("No se creo el bucket" + ex);
            }
        }

        [HttpDelete]
        [Route("DeleteBucketMINio")]
        public async Task<IActionResult> DeleteBucketMINio(string name)
        {
            try
            {
                bool found = await _fileManagerFactory.GetMinio().BucketExistsAsync(name);
                if (found)
                {
                    await _fileManagerFactory.GetMinio().RemoveBucketAsync(name);
                }
                else
                {
                    return Conflict("No se borro el bucket, NO EXISTE!!!");
                }

                var listBucketResponse = await _fileManagerFactory.GetMinio().ListBucketsAsync();

                var lista = "";

                foreach (var bucket in listBucketResponse.Buckets)
                {
                    lista += "\n bucket '" + bucket.Name + "' created at " + bucket.CreationDate + "\n";
                }

                return Ok("Se borro el bucket: " + lista);
            }
            catch (Exception ex)
            {
                return Conflict("No se borror el bucket" + ex);
            }
        }

        //private byte[] ReadAllBytes(IFormFile file)
        //{
        //    byte[] buffer = null;
        //    using (FileStream fs = new FileStream(file.Name, FileMode.Open, FileAccess.Read))
        //    {
        //        buffer = new byte[fs.Length];
        //        fs.Read(buffer, 0, (int)fs.Length);
        //    }
        //    return buffer;
        //}

        [HttpPost]
        [Route("CreateObjectMINio")]
        public async Task<IActionResult> CreateObjectMINio(string nameBucket, string nameObject, IFormFile file)
        {
            if (file.Length <= 0)
                return BadRequest("Empty file");

            try
            {
                //var _file = ReadAllBytes(file);
                var contentType = "image/jpeg";

                //MemoryStream filestream = new(_file);

                //byte[] bs = System.IO.File.ReadAllBytes(fileName);
                //System.IO.MemoryStream filestream = new System.IO.MemoryStream(bs);

                PutObjectArgs args = new PutObjectArgs()
                                                    .WithBucket(nameBucket)
                                                    .WithObject(nameObject)
                                                    .WithStreamData(file.OpenReadStream())
                                                    .WithObjectSize(file.OpenReadStream().Length)
                                                    .WithContentType(contentType)
                                                    //.WithHeaders(metaData)
                                                    .WithServerSideEncryption(null);

                await _fileManagerFactory.GetMinio().PutObjectAsync(args);

                return Ok("se creo la imagen!!!");
            }
            catch (Exception ex)
            {
                return Conflict($"No se encontro el object: {nameObject}" + ex);
            }
        }

        [HttpGet]
        [Route("GetObjectMINio")]
        public async Task<IActionResult> GetObjectMINio(string nameBucket, string nameObject)
        {
            byte[] bytesFromPhoto = null;
            try
            {
                

                await _fileManagerFactory.GetMinio().StatObjectAsync(nameBucket, nameObject);

                await _fileManagerFactory.GetMinio().GetObjectAsync(nameBucket, nameObject,
                                    async (stream) =>
                                    {
                                        //stream.CopyToAsync(Console.OpenStandardOutput());

                                        //string path = Path.Combine(_environment.ContentRootPath, "Images", $"{nameObject}.jpg");
                                        //using (var stream_fs = new FileStream(path, FileMode.Create))
                                        //{
                                        //    await stream.CopyToAsync(stream_fs);
                                        //}
                                        byte[] buffer = new byte[16 * 1024];
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            int read;
                                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                            {
                                                ms.Write(buffer, 0, read);
                                            }
                                            bytesFromPhoto = ms.ToArray();
                                        }
                                    });
                Byte[] bphoto = bytesFromPhoto;
                //bphoto = await System.IO.File.ReadAllBytesAsync(Path.Combine(_environment.ContentRootPath, "Images", $"{nameObject}.jpg"));
                return File(fileContents: bphoto, "image/jpeg");
            }
            catch (Exception ex)
            {
                return Conflict($"No se encontro el object: {nameObject}" + ex);
            }
        }
    }
}
