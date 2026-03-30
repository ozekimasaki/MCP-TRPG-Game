using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Game.Service.Data;
using ToolBox.Tools.GameTools;
using ModelContextProtocol.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Game.Service.Interface;
using Game.Service.Services;
using Common.Interface;
using Common.Services;
using Common.Localization;
using ToolBox.Tools;
using ToolBox.Middleware;
using ToolBox.Tools.GamePrompt;

// Decide mode by command-line: pass --stdio to run stdio transport, otherwise http
var mode = args.Contains("--stdio") ? "stdio" : "http";

if (mode == "http")
{
	var builder = WebApplication.CreateBuilder(args);

	builder.Logging.AddConsole(consoleLogOptions =>
	{
		consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
	});

	builder.WebHost.UseUrls(builder.Configuration["Hosting:Url"] ?? "http://localhost:5000");

	RegisterCommonServices(builder.Services, builder.Configuration);
	builder.Services.AddScoped<McpContextMiddleware>();
	builder.Services.AddScoped<McpResponseFlattenerMiddleware>();
	builder.Services.AddHttpContextAccessor();
	builder.Services.AddMcpServer(_ => { }).WithToolsFromAssembly().WithHttpTransport();

	var app = builder.Build();

	app.UseMiddleware<McpContextMiddleware>();
	app.UseMiddleware<McpResponseFlattenerMiddleware>();
	app.MapMcp("/mcp");
	InitializeAndSeed(app.Services);

	await app.RunAsync();
}
else
{
	var builder = Host.CreateApplicationBuilder(args);

	builder.Logging.AddConsole(consoleLogOptions =>
	{
		consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
	});

	RegisterCommonServices(builder.Services, builder.Configuration);

	Console.SetOut(new StdioMcpResponseFlattener(Console.Out));

	builder.Services
		.AddMcpServer()
		.WithStdioServerTransport()
		.WithToolsFromAssembly();

	var host = builder.Build();

	InitializeAndSeed(host.Services);

	await host.RunAsync();
}

static void RegisterCommonServices(IServiceCollection services, IConfiguration configuration)
{
	var connectionString = configuration["ConnectionStrings:DefaultConnection"]
		?? "Data Source=trpg.db;";

	services.AddDbContext<TrpgDbContext>(options =>
		options.UseSqlite(connectionString));

	services.AddSingleton(new GameLanguageOptions
	{
		DefaultLanguage = configuration["Localization:DefaultLanguage"] ?? "ja-JP"
	});

	services.AddScoped<DbContext>(sp => sp.GetRequiredService<TrpgDbContext>());

	services.AddScoped<IKPService, KPService>();
	services.AddScoped<ICharacterService, CharacterService>();
	services.AddScoped<ICheckService, CheckService>();
	services.AddScoped<IScenarioService, ScenarioService>();
	services.AddScoped<ISeedDataLoader, SeedDataLoader>();
}

static void InitializeAndSeed(IServiceProvider services)
{
	TrpgTools.Initialize(services);
	TrpgPrompt.Initialize(services);

	using (var scope = services.CreateScope())
	{
		var context = scope.ServiceProvider.GetRequiredService<TrpgDbContext>();

		context.Database.EnsureCreated();
		try
		{
			var seedLoader = scope.ServiceProvider.GetRequiredService<ISeedDataLoader>();
			seedLoader.LoadAllSeedData();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[SeedDataLoader] {ex.Message}");
		}
	}
}
