using Microsoft.AspNetCore.HttpOverrides;

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
        });
});

WebApplication app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();  // redirect 80 to 443
app.UseDefaultFiles();      // use index.html & index.css
app.UseStaticFiles();       // enable static file serving
app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/next", async Task<IResult> (HttpRequest request) =>
{
    return "next";
});

app.MapGet("/prev", async Task<IResult> (HttpRequest request) =>
{
    return "prev";
});

app.MapGet("/random", async Task<IResult> (HttpRequest request) =>
{
    return "random";
});

List<Website> websites = new();

string[] lines = File.ReadAllLines("list.txt");
foreach (string line in lines)
{
    Website website = new Website
    {
        username = line.Split("\t").First(),
        domains = line.Split("\t").Last().Split(",").ToList()
    };

    websites.Add(website);
}

app.Run();

public class Website
{
    public string? username;
    public List<string>? domains;
}