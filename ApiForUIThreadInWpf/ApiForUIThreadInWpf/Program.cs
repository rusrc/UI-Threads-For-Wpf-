var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

#region dev info.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); 
#endregion


app.MapPost("/slow-endpoint", (object obj) =>
{
    Task.Delay(5000).Wait(); // Thread.Sleep(5000);

    return Results.Ok($"Time is {DateTime.Now:T}");
});

app.Run();
