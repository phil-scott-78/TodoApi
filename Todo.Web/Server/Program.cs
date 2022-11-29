using System.Text;
using MonorailCss;
using Todo.Web.Server;

var builder = WebApplication.CreateBuilder(args);

// Configure auth with the front end
builder.Services.AddAuthentication().AddCookie(o =>
{
    o.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddAuthorizationBuilder();

// Add razor pages so we can render the Blazor WASM todo component
builder.Services.AddRazorPages();

// Add the forwarder to make sending requests to the backend easier
builder.Services.AddHttpForwarder();

var todoUrl = builder.Configuration.GetServiceUri("todoapi")?.ToString() ??
              builder.Configuration["TodoApiUrl"] ?? 
              throw new InvalidOperationException("Todo API URL is not configured");

// Configure the HttpClient for the backend API
builder.Services.AddHttpClient<TodoClient>(client =>
{
    client.BaseAddress = new(todoUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapFallbackToPage("/_Host");

// Configure the APIs
app.MapAuth();
app.MapTodos(todoUrl);
app.MapGet("/styles.css", () =>
{
    var framework = new CssFramework(new CssFrameworkSettings()
    {
        Applies = new Dictionary<string, string>()
        {
            { "html", "font-sans" },
            { "input.valid.modified:not([type=checkbox])", "ring-green-400 border-green-400" },
            { "input.invalid", "ring-red-400 border-red-400" },
            { "input.invalid:focus", "ring-red-400 border-red-400" },
            { "#blazor-error-ui", "bg-yellow-100 text-yellow-900 hidden fixed bottom-0 left-0 w-full p-2"}
        },
    });
    var css = framework.Process(Todo.Web.Client.Monorail.CssClassValues());
    return Results.Text(css, "text/css", Encoding.Default);
});

app.Run();

