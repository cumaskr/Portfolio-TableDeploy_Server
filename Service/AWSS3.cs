using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using dotenv.net;

namespace APIServer
{
    /// <summary>
    /// AWS S3(저장소) 클래스[whjeon 24.02.14]
    /// 사용용도 : 서버에서 특정 버전 테이블을 읽어 데이터 사용
    /// 사용방법
    /// 1).env 파일에 AWS 관련 인증정보 수동 입력(GitHub에는 빈파일 올라감)
    /// 2)해당 클래스 사용
    ///클라이언트 프로젝트와 비슷한 구조이며, 후에 공용화 해보기
    public class AWSS3 : IDisposable, IExternalStore
    {
        //AWS 서비스 사용하기위한 클라이언트 객체
        public AmazonS3Client? Client { get; set; } = null;

        private const string bucketName = "asia-table";

        public AWSS3()
        {
            try
            {
                Console.WriteLine("[AWS] Start Setting");

                //환경변수 불러오기
                DotEnv.Load();
                //클라이언트 생성
                Client = new AmazonS3Client(
                    Environment.GetEnvironmentVariable("AWS_ID"),
                    Environment.GetEnvironmentVariable("AWS_Key"),
                    new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.APNortheast2,
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Client = null;
                return;
            }
        }

        //S3로부터 특정테이블의 특정버전 파일을 받는다.
        public void DownloadTableFile(string folderName, int version)
        {
            if (null != Client)
            {
                var req = new GetObjectRequest()
                {
                    BucketName = bucketName,
                    Key = $"{folderName}/{version}.{Const.FileExtension.Json}",
                };
                var resObj = Client.GetObjectAsync(req).GetAwaiter().GetResult();

                using (StreamReader reader = new StreamReader(resObj.ResponseStream))
                {
                    string content = reader.ReadToEnd();
                    Console.WriteLine(content);
                }
            }
        }

        //리소스 정리
        public void Dispose()
        {
            if (null != Client)
            {
                Client.Dispose();
            }
        }
    }
}
