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
        private string AdminSdkJson = "tablebuild-e6f20-firebase-adminsdk-xx0ap-d795853497.json";
        private string ProjectId = "tablebuild-e6f20";
        private string Server = "Local";
        private string DocumentName = "Table";

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

            var path = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                logger.LogInformation("[FireBase] OS:Winndows");
                path = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{AdminSdkJson}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                logger.LogInformation("[FireBase] OS:Linux");
                path = $"./../../../{AdminSdkJson}";
            }

            //파이어베이스 리스너 등록
            if (false == string.IsNullOrEmpty(path) && File.Exists(path)) 
            {
                logger.LogInformation("[FireBase] Start Setting");

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
                Db = FirestoreDb.Create(ProjectId);
                Listner = Db.Collection(Server).Document(DocumentName).Listen(snapshot =>
                {
                    var dic = snapshot.ToDictionary();
                    var str = JsonConvert.SerializeObject(dic);
                    logger.LogInformation($"[FireBase] Call Back - {str}");
                });

                logger.LogInformation("[FireBase] Register Listner Done");
            }
            else
            {
                logger.LogInformation("[FireBase] Fail Setting");
            }
        }
    }
}
