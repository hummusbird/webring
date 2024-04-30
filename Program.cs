using Microsoft.AspNetCore.HttpOverrides;

List<Website> websites = new();
var random = new Random();

string[] lines = File.ReadAllLines("list.txt");
foreach (string line in lines)
{
    Website website = new Website
    {
        username = line.Split(";").First(),
        domains = line.Split(";").Last().Split(",").ToList()
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
    if (request.Headers["Referer"].ToString() == "")
    { // no referer, go to random domain
        Console.WriteLine("/next: no referrer, picking random website");
        return Results.Redirect(websites[random.Next(websites.Count)].domains.First());
    }

    for (int i = 0; i < websites.Count; i++)
    {
        if (websites[i].domains.Contains(request.Headers["Referer"])) // found website
        {
            Console.WriteLine($"/next: next domain in webring from {request.Headers["Referer"]}");
            if (i == websites.Count) { i = -1; }
            return Results.Redirect(websites[i + 1].domains.First());
        }
    }

    Console.WriteLine($"/prev: couldn't find {request.Headers["Referer"]} in list, picking random website");
    return Results.Redirect(websites[random.Next(websites.Count)].domains.First()); // unable to find website in list
});

app.MapGet("/prev", async Task<IResult> (HttpRequest request) =>
{
    if (request.Headers["Referer"].ToString() == "")
    { // no referer, go to random domain
        Console.WriteLine("/prev: no referrer, picking random website");
        return Results.Redirect(websites[random.Next(websites.Count)].domains.First());
    }

    for (int i = 0; i < websites.Count; i++)
    {
        if (websites[i].domains.Contains(request.Headers["Referer"])) // found website
        {
            Console.WriteLine($"/prev: prev domain in webring from {request.Headers["Referer"]}");
            if (i == 0) { i = websites.Count + 1; }
            return Results.Redirect(websites[i - 1].domains.First());
        }
    }

    Console.WriteLine($"/prev: couldn't find {request.Headers["Referer"]} in list, picking random website");
    return Results.Redirect(websites[random.Next(websites.Count)].domains.First()); // unable to find website in list
});

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