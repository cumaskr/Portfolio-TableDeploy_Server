namespace APIServer
{
    /// <summary>
    /// 외부 저장소 사용 시, 꼭 필요한 기능들을 정의
    /// </summary>
    public interface IExternalStore
    {
        //리소스 사용후 바로 반환하기 위하여, 사용한 라이브러리의 Dispose를 호출해준다.
        public void Dispose();        
    }
}
