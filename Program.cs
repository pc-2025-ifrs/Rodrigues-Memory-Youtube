using Microsoft.EntityFrameworkCore;
using YouTube.Data;
using YouTube.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


var usePersistentStorage = builder.Configuration.GetValue<bool>("UsePersistentStorage");

Console.WriteLine($"🗄️ Modo de armazenamento: {(usePersistentStorage ? "PERSISTENTE (SQLite)" : "EM MEMÓRIA")}");

if (usePersistentStorage)
{

    
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    Console.WriteLine($"📊 Provider: SQLite");
    Console.WriteLine($"🔗 Connection: {connectionString}");
    
    builder.Services.AddDbContext<YouTubeDbContext>(options =>
        options.UseSqlite(connectionString));
    
    builder.Services.AddScoped<IAccountRepository, PersistentAccountRepository>();
    builder.Services.AddScoped<IVideoRepository, PersistentVideoRepository>();
    builder.Services.AddScoped<ICommentRepository, PersistentCommentRepository>();
}
else
{

    Console.WriteLine("⚠️ Atenção: Dados serão perdidos ao reiniciar a aplicação!");
    
    builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
    builder.Services.AddSingleton<IVideoRepository, InMemoryVideoRepository>();
    builder.Services.AddSingleton<ICommentRepository, InMemoryCommentRepository>();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "YouTube API",
        Version = "v1",
        Description = "API para gerenciar contas, vídeos e comentários estilo YouTube (com persistência em banco de dados)"
    });
});

var app = builder.Build();


if (usePersistentStorage)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<YouTubeDbContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Aplicando migrations...");
            context.Database.Migrate();
            
            logger.LogInformation("✅ Banco de dados inicializado com sucesso");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "❌ Erro ao inicializar o banco de dados");
            
            if (builder.Environment.IsDevelopment())
            {
                logger.LogWarning("Tentando recriar o banco de dados...");
                try
                {
                    var context = services.GetRequiredService<YouTubeDbContext>();
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    logger.LogInformation("✅ Banco recriado com sucesso");
                }
                catch (Exception recreateEx)
                {
                    logger.LogError(recreateEx, "❌ Falha ao recriar banco");
                }
            }
        }
    }
}
else
{
    Console.WriteLine("ℹ️ Modo em memória - não há banco de dados para inicializar");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "YouTube API v1");
        options.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();