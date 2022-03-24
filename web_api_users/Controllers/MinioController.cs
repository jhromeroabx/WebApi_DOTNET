using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using System;
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

        public MinioController(IFileManagerFactory fileManagerFactory)
        {
            _fileManagerFactory = fileManagerFactory;
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

                return Ok("Se creo el bucket: "+ lista);
            }
            catch (Exception ex)
            {
                return Conflict("No se creo el bucket" + ex);
            }
        }

        [HttpPost]
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
    }
}
