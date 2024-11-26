using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddAuthorization();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});


// Add HttpContext to .Common project (IHttpContextAccessor) into the DI
builder.Services.AddHttpContextAccessor();

const string corsAllowAnyone = "AllowAnything";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsAllowAnyone,
        policy => { policy.WithOrigins().AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
});

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHttpForwarder();

var app = builder.Build();

app.UseHsts();
app.UseCors(corsAllowAnyone);
app.UseResponseCompression();
app.UseStatusCodePages();
#if !DEBUG
app.UseHttpsRedirection(); // needed for docusaurus
#endif
app.UseStaticFiles();

// maps swagger as endpoints
app.MapSwagger();

app.UseRouting();
// app.UseSystemWebAdapters();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();
app.Run();



