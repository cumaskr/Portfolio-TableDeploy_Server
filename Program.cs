using APIServer;

var builder = WebApplication.CreateBuilder(args);

//컨트롤러 기능 추가
builder.Services.AddControllers();

//파이어베이스 서버 추가(리스너 등록) - 로그추가
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddFilter("System", LogLevel.Trace).AddConsole();
});
var fs = new FireBase(loggerFactory.CreateLogger<FireBase>());
builder.Services.AddSingleton(fs);

//서버 빌드(셋팅)
var app = builder.Build();

//연습)index.html 파일 기본 보여주기위해 추가, 주의 순서가 변경되면 안됨!
app.UseDefaultFiles();
app.UseStaticFiles();

//Controller 매핑
app.MapControllers();

//서버 시작
app.Run();