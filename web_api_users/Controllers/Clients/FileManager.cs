using Minio;

namespace web_api_users.Controllers.Clients
{
    public class FileManager : IFileManagerFactory
    {
        MinioClient minio = null;

        public MinioClient GetMinio()
        {
            return minio;
        }

        public void SetupMinio(MinioClient minio)
        {
            // validacion : si ya existe el objeto minio no sobreescribirlo.. (if)
            this.minio = minio;
        }

    }
}
