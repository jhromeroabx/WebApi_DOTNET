using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using web_api_users.Controllers.Clients;
using web_api_users.Controllers.ParamsDTO;

namespace web_api_users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MinioController : ControllerBase
    {
        private readonly IFileManagerFactory _fileManagerFactory;
        //private readonly IWebHostEnvironment _environment;

        public MinioController(IFileManagerFactory fileManagerFactory, IWebHostEnvironment environment)
        {
            _fileManagerFactory = fileManagerFactory;
            //_environment = environment;
        }

        [HttpPost]
        [Route("CreateBucketMINio")]
        public async Task<IActionResult> CreateBucketMINio(string name)
        {
            try
            {
                BucketExistsArgs bucketExistsArgs = new BucketExistsArgs().WithBucket(name);

                bool found = await _fileManagerFactory.GetMinio().BucketExistsAsync(bucketExistsArgs);
                if (!found)
                {
                    MakeBucketArgs makeBucketArgs = new MakeBucketArgs()
                        .WithBucket(name)
                        .WithLocation("/Data");

                    await _fileManagerFactory.GetMinio().MakeBucketAsync(makeBucketArgs);
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

        [HttpGet]
        [Route("ListBucketsMINio")]
        public async Task<IActionResult> ListBucketsMINio()
        {
            try
            {
                var listBucketResponse = await _fileManagerFactory.GetMinio().ListBucketsAsync();

                List<BucketInfo> lista = new();

                foreach (var bucket in listBucketResponse.Buckets)
                {
                    //lista += "\n bucket '" + bucket.Name + "' created at " + bucket.CreationDate + "\n";
                    lista.Add(
                         new BucketInfo
                         {
                             Name = bucket.Name,
                             CreationDate = bucket.CreationDateDateTime
                         }
                        );
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return Conflict("No se listo los busckets: " + ex);
            }
        }

        [HttpGet]
        [Route("ListObjectsMINio")]
        public async Task<IActionResult> ListObjectsMINio(string name)
        {
            try
            {
                var lista = "";
                BucketExistsArgs bucketExistsArgs = new BucketExistsArgs().WithBucket(name);

                bool found = await _fileManagerFactory.GetMinio().BucketExistsAsync(bucketExistsArgs);
                if (found)
                {


                    ListObjectsArgs args = new ListObjectsArgs()
                                              .WithBucket(name)
                                              //.WithPrefix("prefix")
                                              .WithRecursive(true);
                    IObservable<Item> observable = _fileManagerFactory.GetMinio().ListObjectsAsync(args);
                    var completionSource = new TaskCompletionSource<string>();

                    var isObservableEmpty = !await observable.Any();

                    if (isObservableEmpty)
                    {
                        return Ok("No hay objetos");
                    }

                    IDisposable subscription = observable.Subscribe(
                        item => lista += "\n object: '" + item.Key + "' created or modified at " + item.LastModifiedDateTime + "\n",
                        ex => lista += "\n Error ocurred:" + ex.Message + " \n",
                        () =>
                        {
                            lista += "OnComplete: {0}";
                            completionSource.SetResult(lista);
                        });

                    await completionSource.Task;

                    return Ok(lista);
                }
                else
                {
                    return Conflict("El bucket no existe!");
                }
            }
            catch (MinioException ex)
            {
                return Conflict($"No se listo los objetos del bucket {name}: " + ex);
            }
        }

        [HttpDelete]
        [Route("DeleteBucketMINio")]
        public async Task<IActionResult> DeleteBucketMINio(string name)
        {
            try
            {
                BucketExistsArgs bucketExistsArgs = new BucketExistsArgs().WithBucket(name);

                bool found = await _fileManagerFactory.GetMinio().BucketExistsAsync(bucketExistsArgs);
                if (found)
                {
                    RemoveBucketArgs removeBucketArgs = new RemoveBucketArgs().WithBucket(name);

                    await _fileManagerFactory.GetMinio().RemoveBucketAsync(removeBucketArgs);
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
        public async Task<IActionResult> CreateObjectMINio(string nameBucket, string nameObject, string contentType, IFormFile file)
        {
            if (file.Length <= 0)
                return BadRequest("Empty file");

            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs().
                                                    WithBucket(nameBucket).
                                                    WithObject(nameObject);

                var obj = await _fileManagerFactory.GetMinio().StatObjectAsync(statObjectArgs);

                return Conflict($"Se encontro el object: {nameObject} existente, no se puede reemplazar, cambie el nombre!");
            }
            catch (Minio.Exceptions.ObjectNotFoundException)
            {
                List<string> allowedContentTypes = new()
                {
                "image/apng",
                "image/avif",
                "image/jpeg",
                "image/png",
                "image/svg+xml",
                "image/webp"
                };

                int width = 0;
                int height = 0;

                // Si el archivo es un tipo de imagen permitido
                if (allowedContentTypes.Contains(contentType))
                {
                    int maxResolutionWidth = 1200;
                    int maxResolutionHeight = 600;

                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        using (var image = new Bitmap(memoryStream))
                        {
                            if (image.Width > maxResolutionWidth && image.Height > maxResolutionHeight)
                            {
                                // Reducción de resolución utilizando Bitmap
                                using var reducedImage = new Bitmap(image, maxResolutionWidth, maxResolutionHeight);
                                using var memoryStreamNext = new MemoryStream();
                                reducedImage.Save(memoryStreamNext, ImageFormat.Jpeg); // Cambia el formato según tus necesidades
                                memoryStreamNext.Position = 0;
                                // Continuar con el procesamiento y almacenamiento del objeto reducido
                                await ProcessAndStoreImage(nameBucket, nameObject, contentType, memoryStreamNext);
                                return Ok("¡Se creó la imagen!");
                            }
                            else
                            {
                                // Continuar con el procesamiento y almacenamiento del objeto original
                                using var stream = file.OpenReadStream();
                                await ProcessAndStoreImage(nameBucket, nameObject, contentType, stream);
                                return Ok("¡Se creó la imagen!");
                            }
                        }
                    }
                }
                else
                {
                    // Continuar con el procesamiento y almacenamiento del objeto original
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        await ProcessAndStoreImage(nameBucket, nameObject, contentType, memoryStream);
                        return Ok("¡Se creó la imagen!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict($"No se creo el object: {nameObject} - el error es:" + ex);
            }
        }

        private async Task ProcessAndStoreImage(string nameBucket, string nameObject, string contentType, Stream stream)
        {
            // Puedes realizar cualquier procesamiento adicional necesario antes de almacenar la imagen
            // ...

            PutObjectArgs args = new PutObjectArgs()
                .WithBucket(nameBucket)
                .WithObject(nameObject)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType)
                .WithServerSideEncryption(null);

            await _fileManagerFactory.GetMinio().PutObjectAsync(args);
        }

        [HttpGet]
        [Route("GetObjectMINio")]
        public async Task<IActionResult> GetObjectMINio(string nameBucket, string nameObject)
        {
            byte[] bytesFromPhoto = null;
            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs().WithBucket(nameBucket).WithObject(nameObject);

                await _fileManagerFactory.GetMinio().StatObjectAsync(statObjectArgs);

                GetObjectArgs args = new GetObjectArgs().WithCallbackStream((stream) =>
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

        [HttpDelete]
        [Route("DeleteObjectMINio")]
        public async Task<IActionResult> DeleteObjectMINio(string bucket, string objectname)
        {
            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs().WithBucket(bucket).WithObject(objectname);

                await _fileManagerFactory.GetMinio().StatObjectAsync(statObjectArgs);

                RemoveObjectArgs rmArgs = new RemoveObjectArgs()
                                              .WithBucket(bucket)
                                              .WithObject(objectname);
                await _fileManagerFactory.GetMinio().RemoveObjectAsync(rmArgs);

                return Ok($"se borro el object {objectname}");
            }
            catch (Exception ex)
            {
                return Conflict($"No se borro el object: {objectname}: -" + ex);
            }
        }
    }
}
