var builder = WebApplication.CreateBuilder(args);

//컨트롤러 기능 추가
builder.Services.AddControllers();

//서버 셋팅
var app = builder.Build();

//연습)index.html 파일 기본 보여주기위해 추가, 주의 순서가 변경되면 안됨!
app.UseDefaultFiles();
app.UseStaticFiles();

//Controller 매핑
app.MapControllers();

//서버 시작
app.Run();