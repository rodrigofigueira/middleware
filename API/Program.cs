using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var timer = new Stopwatch();
    timer.Start();

    string protocol = context.Request.IsHttps ? "https" : "http";
    string traceIdentifier = context.TraceIdentifier;

    Console.WriteLine($"\n[START] TraceIdentifier: {traceIdentifier}");
    Console.WriteLine($"[INFO] Path: {context.Request.Path}");
    Console.WriteLine($"[INFO] Method: {context.Request.Method}");
    Console.WriteLine($"[INFO] Protocol: {protocol}");
    Console.WriteLine($"[INFO] Host: {context.Request.Host}");
    Console.WriteLine($"[INFO] Agent: {context.Request.Headers.UserAgent}");

    await next(context);
    timer.Stop();
    var timeTaken = timer.Elapsed;

    Console.WriteLine($"[PERFORMANCE] The request {traceIdentifier} took {timeTaken}");
    Console.WriteLine($"[INFO] Status Code Response: {context.Response.StatusCode}");
    Console.WriteLine($"[END] The request {traceIdentifier}");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
