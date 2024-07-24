using Newtonsoft.Json;
using Microsoft.AspNetCore.HttpOverrides;

List<Website> websites = new();
var random = new Random();

var env = Environment.GetEnvironmentVariable("WEBRING_LIST");
string[] lines = env == null ? File.ReadAllLines("list.txt") : env.Split("\n");
foreach (string line in lines)
{
    string url = null;
    if (line.Split(";").Count() > 2) { url = line.Split(";").Last(); } // if there's two splits, the second is the badge url

    Website website = new Website
    {
        username = line.Split(";")[0],
        badgeurl = url,
        domains = line.Split(";")[1].Split(",").ToList()
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

app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", () =>
{
    string html = File.ReadAllText("wwwroot/index.html");

    string members = "";
    foreach (Website website in websites)
    {
        members += $"<div><a href=\"{website.domains.First()}\">{website.username}</a><img src=\"{website.badgeurl}\"></div>\n";
    }

    return Results.Content(html.Replace("{{members}}", members), "text/html");
});

app.MapGet("/next", async Task<IResult> (HttpRequest request) =>
{
    if (request.Headers["Referer"].ToString() == "")
    { // no referer, go to random domain
        Console.WriteLine("/next: no referrer, picking random website");
        return Results.Redirect("/rand");
    }

    for (int i = 0; i < websites.Count; i++)
    {
        if (websites[i].domains.Contains(request.Headers["Referer"])) // found website
        {
            Console.WriteLine($"/next: referred from {websites[i].username}");
            if (i == websites.Count - 1) { i = -1; }
            return Results.Redirect(websites[i + 1].domains.First());
        }
    }

    Console.WriteLine($"/prev: couldn't find {request.Headers["Referer"]} in list, picking random website");
    return Results.Redirect("/rand"); // unable to find website in list
});

app.MapGet("/prev", async Task<IResult> (HttpRequest request) =>
{
    if (request.Headers["Referer"].ToString() == "")
    { // no referer, go to random domain
        Console.WriteLine("/prev: no referrer, picking random website");
        return Results.Redirect("/rand");
    }

    for (int i = 0; i < websites.Count; i++)
    {
        if (websites[i].domains.Contains(request.Headers["Referer"])) // found website
        {
            Console.WriteLine($"/prev: referred from {websites[i].username}");
            if (i < 1) { i = websites.Count; }
            return Results.Redirect(websites[i - 1].domains.First());
        }
    }

    Console.WriteLine($"/prev: couldn't find {request.Headers["Referer"]} in list, picking random website");
    return Results.Redirect("/rand"); // unable to find website in list
});

app.MapGet("/rand", () =>
{
    Console.WriteLine("/rand: picking random website");
    return Results.Redirect(websites[random.Next(websites.Count)].domains.First());
});

app.MapGet("/members", () =>
{
    Console.WriteLine("/members: requested members with badges");
    return Results.Text(JsonConvert.SerializeObject(websites));
});

app.Run();

public class Website
{
    public string? username;
    public string? badgeurl;
    public List<string>? domains;
}