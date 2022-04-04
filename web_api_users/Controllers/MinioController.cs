using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using System;
using System.IO;
using System.Threading.Tasks;
using web_api_users.Controllers.Clients;

namespace web_api_users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MinioController : ControllerBase
    {
        private readonly IFileManagerFactory _fileManagerFactory;
        private readonly IWebHostEnvironment _environment;

        public MinioController(IFileManagerFactory fileManagerFactory, IWebHostEnvironment environment)
        {
            _fileManagerFactory = fileManagerFactory;
            _environment = environment;
        }

        //[HttpPost]
        //[Route("ConnectToMINio")]
        //public async Task<IActionResult> ConnectToMINio([FromBody] CredentialsMINio credentials, bool hard_soft)
        ///*
        //    {
        //        "endpoint": "192.168.1.17:9000",
        //        "accessKey": "admin",
        //        "secretKey": "password"
        //    }
        // */
        //{
        //    try
        //    {
        //        if (hard_soft)
        //        {
        //            _fileManagerFactory.SetupMinioHard();
        //        }
        //        else
        //        {
        //            _fileManagerFactory.SetupMinio(new MinioClient()
        //                                    .WithEndpoint(credentials.endpoint)
        //                                    .WithCredentials(credentials.accessKey,
        //                                             credentials.secretKey)
        //                                    //.WithSSL()
        //                                    .Build());
        //        }

        //        var listBucketResponse = await _fileManagerFactory.GetMinio().ListBucketsAsync();

        //        var lista = "";

        //        foreach (var bucket in listBucketResponse.Buckets)
        //        {
        //            lista += "\n bucket '" + bucket.Name + "' created at " + bucket.CreationDate + "\n";
        //        }

        //        return Ok("Conecto al MYMINIO: " + lista);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Conflict("No Conecto al MYMINIO" + ex);
        //    }
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
                    await _fileManagerFactory.GetMinio().MakeBucketAsync(name, "/Data");
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
                StatObjectArgs args_stat = new StatObjectArgs().WithBucket(nameBucket).WithObject(nameObject);

                await _fileManagerFactory.GetMinio().StatObjectAsync(args_stat);

                GetObjectArgs args = new GetObjectArgs().WithCallbackStream(async (stream) =>
                {
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
                })
                    .WithBucket(nameBucket)
                    .WithObject(nameObject);

                await _fileManagerFactory.GetMinio().GetObjectAsync(args);
                byte[] bphoto = bytesFromPhoto;
                return File(fileContents: bphoto, "image/jpeg");
            }
            catch (Exception ex)
            {
                return Conflict($"No se encontro el object: {nameObject}" + ex);
            }
        }
    }
}
