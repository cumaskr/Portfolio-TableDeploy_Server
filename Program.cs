var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//연습)index.html 파일 기본 보여주기위해 추가
//주의 순서가 변경되면 안됨!
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();