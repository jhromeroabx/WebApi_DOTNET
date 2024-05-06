using Minio;

namespace web_api_users.Controllers.Clients
{
    public class FileManager : IFileManagerFactory
    {
        MinioClient minio = null;

        public FileManager()
        {
            this.minio = new MinioClient()
                                    .WithEndpoint("192.168.18.96:8530")
                                    .WithCredentials("123wasd",
                                             "123wasd@wasd")
                                    .Build();
        }

        public MinioClient GetMinio()
        {
            return minio;
        }

        //public void SetupMinio(MinioClient minio)
        //{
        //    if (this.minio == null)
        //        this.minio = minio;
        //}

        //public void SetupMinioHard()
        //{
        //    if (this.minio == null)
        //        this.minio = new MinioClient()
        //                            .WithEndpoint("192.168.18.6:8500")
        //                            .WithCredentials("loasi.wastore",
        //                                     "loasi.wastore@wasd12125")
        //                            .Build();
        //}
    }
}
