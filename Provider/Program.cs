using Provider;

var builder = WebApplication.CreateBuilder(args)
    .ConfigureServices();

var app = builder.Build();

app.Configure();

app.Run();
