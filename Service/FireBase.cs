using Google.Cloud.Firestore;
using System.Runtime.InteropServices;

namespace APIServer
{
    /// <summary>
    /// FireBase(저장소) 클래스[whjeon 24.02.13]
    ///클라이언트 프로젝트와 비슷한 구조이며, 후에 공용화 해보기
    /// 사용방법
    /// 1)FireBase.json 파일에 인증값 수동으로 입력(GitHub에는 빈파일 올라감)
    /// 2)해당 클래스 사용
    /// </summary>
    public class FireBase : IDisposable
    {
        //저장소 접근하기위한 Json파일명(해당 파일은 깃에 올라가지 않음, 특정 컴퓨터 내부에서 관리)
        private string AdminSdkJson = "FireBase.json";
        private string ProjectId = "tablebuild-e6f20";
        private string Server = "Local";
        private string DocumentName = "Table";

        //테이블 변경 로직 동기처리 오브젝트
        private object LockTableChange = new object();

        //테이블 데이터(최종 변환된 테이블을 담는 클래스)
        public DataClient DataClient;
        //테이블별 최신버전 담을 컨테이너
        private Dictionary<string, int>? TableRecentVersions;
        //저장소 객체
        public FirestoreDb? Db { get; private set; }
        //저장소 리스너
        private FirestoreChangeListener? Listner { get; set; }
        //로그
        private ILogger Logger { get; set; }

        public FireBase(ILogger logger)
        {
            //로그 셋팅
            Logger = logger;
            //로그
            logger.LogInformation("[FireBase] Start Setting");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                logger.LogInformation("[FireBase] OS:Winndows");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                logger.LogInformation("[FireBase] OS:Linux");
            }
            else
            {
                logger.LogInformation("[FireBase] UnKnown OS");
            }

            //------------------------------------------------------------1.DataClient 생성
            DataClient = new DataClient(logger);
            //------------------------------------------------------------2.FireBase 인증정보 셋팅
            var path = $"./{AdminSdkJson}";
            try
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
                Db = FirestoreDb.Create(ProjectId);

                logger.LogInformation("[FireBase] Registe Listner");

                TableRecentVersions = new Dictionary<string, int>();

                //------------------------------------------------------------3.콜백 등록(서버 로드 시, 한번 무조건 호출됨 혹은 저장소 변경 시)
                //테이블 다운로드가 오래걸릴 경우, 동시에 다른 버전 요청 받아놓기위해 비동기로 변경
                Listner = Db.Collection(Server).Document(DocumentName).Listen(async snapshot =>
                {
                    //------------------------------------------------------------4.AWS S3 생성
                    var aws = new AWSS3(logger);
                    try
                    {
                        lock (LockTableChange)
                        {
                            //------------------------------------------------------------5.전달받은 버전파일로 S3로부터 Json파일을 받는다.
                            //테이블 별, 최신버전 목록
                            var resVersions = snapshot.ToDictionary();
                            //테이블 순회
                            foreach (var item in resVersions)
                            {
                                //전달받은 버전
                                var resVersion = Convert.ToInt32(item.Value);
                                //컨테이너에 관리하는 버전과 비교하여, 받은 버전이 높다면 AWS 다운로드 로직으로 넘어간다.
                                if (TableRecentVersions.TryGetValue(item.Key, out var nowVersion))
                                {
                                    if (nowVersion >= resVersion)
                                    {
                                        continue;
                                    }

                                    TableRecentVersions[item.Key] = resVersion;
                                }
                                else
                                {
                                    TableRecentVersions.Add(item.Key, resVersion);
                                }

                                logger.LogInformation($"[FireBase] Table Change - {item.Key} {resVersion}");

                                //AWS S3에서 테이블 파일 받기
                                var json = aws.DownloadTable(item.Key, resVersion);
                                if (false == string.IsNullOrEmpty(json))
                                {
                                    //DataClient에 사용할 데이터 형태로 변환
                                    DataClient.ConvertByReflection(item.Key, json);
                                }
                                else
                                {
                                    logger.LogInformation($"[AWS] Fail Download File - {item.Key} {resVersion}");
                                }
                            }
                            //리소스 해제
                            aws.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogInformation(e, "[FireBase] Table Change Error");
                    }
                    finally 
                    {
                        if (null != aws)
                            aws.Dispose();
                    }
                });
            }
            catch (Exception e)
            {
                logger.LogInformation(e, "[FireBase] Fail Setting");
            }
        }

        //리소스 해제
        public void Dispose()
        {
            Db = null;
        }
    }
}