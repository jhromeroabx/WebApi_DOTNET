using Minio;

namespace web_api_users.Controllers.Clients
{
    public class FileManager : IFileManagerFactory
    {
        MinioClient minio = null;

        public FileManager()
        {
            this.minio = new MinioClient()
                                    .WithEndpoint("192.168.1.2:8500")
                                    .WithCredentials("beedrone.webapi",
                                             "beedrone.webapi123@123")
                                    .Build();
        }

        public MinioClient GetMinio()
        {
            return minio;
        }

        public void SetupMinio(MinioClient minio)
        {
            if (this.minio == null)
                this.minio = minio;
        }

        public void SetupMinioHard()
        {
            if (this.minio == null)
                this.minio = new MinioClient()
                                    .WithEndpoint("192.168.1.2:8500")
                                    .WithCredentials("beedrone.webapi",
                                             "beedrone.webapi123@123")
                                    .Build();
        }
    }
}