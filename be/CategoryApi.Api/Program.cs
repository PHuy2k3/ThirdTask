using CategoryApi.Data;
using CategoryApi.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "FE";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins("http://localhost:3000", "https://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddData(builder.Configuration);
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);
app.MapControllers();
app.Run();
