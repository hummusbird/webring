using Microsoft.AspNetCore.HttpOverrides;

List<Website> websites = new();
var random = new Random();

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
Console.WriteLine($"Loaded {websites.Count} websites.");

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
    if (request.Headers["Referer"].ToString() == null)
    { // no referer, go to random domain
        Console.WriteLine("/next: no referrer, picking random website");
        return Results.Redirect(websites[random.Next(websites.Count)].domains.First());
    }

    Console.WriteLine($"/next: next domain in webring from {request.Headers["Referer"]}");
    return Results.Ok("test");
});

// app.MapGet("/prev", async Task<IResult> (HttpRequest request) =>
// {
//     return "prev";
// });

app.MapGet("/random", () =>
{
    Console.WriteLine("/random: picking random website");
    return Results.Redirect(websites[random.Next(websites.Count)].domains.First());
});

app.Run();

public class Website
{
    public string? username;
    public List<string>? domains;
}