using Minio;

namespace web_api_users.Controllers.Clients
{
    public interface IFileManagerFactory
    {
        //void SetupMinio(MinioClient minio);
        //void SetupMinioHard();
        MinioClient GetMinio();
    }
}
