using Newtonsoft.Json;
using System.Reflection;
using static APIServer.DataStructure;

namespace APIServer
{
    //테이블 형태 클래스
    public class DataStructure
    {
        public class Character
        {
            public int ID { get; set; }
            public int HP { get; set; }
            public int MP { get; set; }
        }

        public class Dungeon
        {
            public int ID { get; set; }
            public int MapIndex { get; set; }
            public int Difficulty { get; set; }
        }
    }

    //테이블 관리 클래스 - 서버에서 테이블을 사용하는 로직들은 이 DataClient를 통하여 사용
    public class DataClient
    {
        //-----------------------------------------------------테이블 데이터
        //※주의 사항 : 꼭 프로퍼티명을 {클래스명}s 형태로 추가 할것![whjeon 24.02.15]
        public Dungeon[]? Dungeons { get; set; }
        public Character[]? Characters { get; set; }
        //-----------------------------------------------------

        private ILogger logger;

        public DataClient(ILogger lg) 
        {
            logger = lg;
        }

        //테이블 변환 중 에러가 났다면 모든 테이블 데이터 Null처리하여 버그 발생 방지
        public void ErrorTables(string errorMsg) 
        {
            logger.LogError($"[DataClient] {errorMsg}");
            Dungeons = null;
            Characters = null;
        }

        /// <summary>
        /// 받아온 Json 파일을 테이블 데이터형태로 변경
        /// 리플렉션을 사용해서 테이블 데이터 프로퍼티만 추가하면 받아올수있도록 구성
        /// 이유 : folderName으로 비교문으로 수동으로 셋팅하면, 테이블이름이 변경될떄마다 변경해야해서 불편
        /// </summary>
        public void ConvertArray<T>(string folderName, string json)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<T[]>(json);
                if (null != result)
                {
                    var propertyInfo = typeof(DataClient).GetProperty($"{folderName}s");
                    if (null != propertyInfo)
                    {
                        propertyInfo.SetValue(this, (T[])((object)result));
                    }
                    else
                    {
                        //새로운 테이블은 업데이트 됬고, 아직 서버의 DataClient에 변환할 클래스를 추가하지 않았다면, 이 경우는 유연하게 처리
                        //즉, 아직 클래스를 정의하지않았다면 테이블 데이터를 사용하는 로직이 없기떄문에 그냥 패스
                        logger.LogInformation($"[DataClient] New Table is Uploaded. But Not Use Table - {folderName}");
                        return;
                    }
                }
                else
                {
                    ErrorTables($"Json Deserialize Fail - {folderName}");
                    return;
                }
            }
            catch (Exception e)
            {
                ErrorTables($"Json Deserialize Fail - {folderName} - {e.Message}");
                return;
            }
        }

        /// <summary>
        /// 받아온 Json 파일을 테이블 데이터형태로 변경
        /// 폴더이름에 대한 클래스 타입을 찾고, 동적으로 변환함수에 타입과 Json을 전달하여 변환 시도
        /// </summary>
        public void ConvertByReflection(string folderName, string json)
        {
            //실행중인 프로그램의 메타정보 참조
            Assembly assembly = Assembly.GetExecutingAssembly();
            //string으로 DataStructure의 알맞은 테이블 클래스타입을 찾아온다.
            Type? tableType = assembly.GetType($"APIServer.DataStructure+{folderName}");
            if (null == tableType)
            {
                ErrorTables("Not Found TableName to Class");
                return;
            }
            //폴더이름으로 Class Type을 찾았다면 알맞은 형태로 변환시킨다.
            var methodInfo = GetType().GetMethod("ConvertArray");
            if (null != methodInfo)
            {
                //변환 함수 실행할떄, 변환하는 클래스Type을 동적으로 넣어주어야해서 함수호출도 리플렉션 사용
                methodInfo.MakeGenericMethod(tableType).Invoke(this, new object[] { folderName, json });
            }
        }
    }
}