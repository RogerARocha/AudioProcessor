using DotNetEnv;
using AudioProcessor.Repositories;
using AudioProcessor.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

//variaveis de ambiente
Env.Load(".env.local");

// supabase 
var url = Env.GetString("SUPABASE_URL");
var key = Env.GetString("SUPABASE_KEY");
builder.Services.AddScoped(_ => new Client(url, key));

//Services
builder.Services.AddScoped<IAudioRepository, AudioRepository>();
builder.Services.AddScoped<IAudioService, AudioService>();
builder.Services.AddScoped<IAudioCompressorService, AudioCompressorService>();
builder.Services.AddScoped<IAudioTranscriptionService, AudioTranscriptionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();