using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System.Runtime.InteropServices;

namespace APIServer
{
    /// <summary>
    /// FireBase(저장소) 클래스[whjeon 24.02.13]
    /// 클라이언트 프로젝트와 설정값은 동일하여, 후에 공용화 해보자
    /// </summary>
    public class FireBaseServer
    {
        //저장소 접근하기위한 Json파일명(해당 파일은 깃에 올라가지 않음, 특정 컴퓨터 내부에서 관리)
        private string AdminSdkJson = "FireBase.json";
        private string ProjectId = "tablebuild-e6f20";
        private string Server = "Local";
        private string DocumentName = "Table";
        //테이블 최신버전 담을 컨테이너
        private Dictionary<string, int>? TableRecentVersions;

        //저장소 객체
        public FirestoreDb? Db { get; private set; }
        //저장소 리스너
        private FirestoreChangeListener? Listner { get; set; }
        //로그
        private ILogger Logger { get; set; }

        public FireBaseServer(ILogger logger)
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
            //인증정보 경로
            var path = $"./{AdminSdkJson}";
            try
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
                Db = FirestoreDb.Create(ProjectId);

                logger.LogInformation("[FireBase] Registe Listner");

                TableRecentVersions = new Dictionary<string, int>();

                //파이어베이스 리스너 등록
                Listner = Db.Collection(Server).Document(DocumentName).Listen(snapshot =>
                {
                    var resVersions = snapshot.ToDictionary();
                    foreach (var item in resVersions) 
                    {
                        var resVersion = Convert.ToInt32(item.Value);
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

                        //TODO AWS에서 파일 받아온다.
                    }
                });
            }
            catch (Exception e)
            {
                logger.LogInformation(e, "[FireBase] Fail Setting");
            }
        }
    }
}