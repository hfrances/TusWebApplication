using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

var store = new TusWebApplication.TusAzureStore("hfcloudlab", "TcYjb+uh2Kme9PcDQHt7BNsIPE2WO8tU0OM13UVpV9+QpqbaVv4Ms60BFuaLtJddyP+NjCrHpFqf+AStQVvl5w==", "default");
var store2 = new TusDiskStore(@"C:\temp\tusfiles");
app.UseTus(httpContext => new DefaultTusConfiguration
{
    // This method is called on each request so different configurations can be returned per user, domain, path etc.
    // Return null to disable tusdotnet for the current request.

    // c:\tusfiles is where to store files
    Store = store,
    // On what url should we listen for uploads?
    UrlPath = "/files",
    MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
    UsePipelinesIfAvailable = true,
    Events = new tusdotnet.Models.Configuration.Events
    {
        OnFileCompleteAsync = async eventContext =>
        {
            ITusFile file = await eventContext.GetFileAsync();
            Dictionary<string, Metadata> metadata = await file.GetMetadataAsync(eventContext.CancellationToken);
            Stream content = await file.GetContentAsync(eventContext.CancellationToken);

            ////await DoSomeProcessing(content, metadata);
            content.ToString();

            var fileName = metadata["filename"].GetString(System.Text.Encoding.UTF8);
            var container = metadata["container"].GetString(System.Text.Encoding.UTF8);
            var factor = metadata["factor"].GetString(System.Text.Encoding.UTF8);
            
            eventContext.ToString();
        }
    }
});

app.MapGet("/files/{fileId}", async httpContext => httpContext.ToString());

app.MapControllers();

app.Run();
