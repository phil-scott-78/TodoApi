using System.Text;
using MonorailCss;

namespace Todo.Web.Server;

internal static class MonorailStyleApi
{
    public static void MapMonorail(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/styles.css", () =>
        {
            var framework = new CssFramework(new()
            {
                // the applies are elements that are more generic or aren't in any razor files
                // that we could grab.
                Applies = new Dictionary<string, string>()
                {
                    { "html", "font-sans" },
                    { "input.valid.modified:not([type=checkbox])", "ring-green-400 border-green-400" },
                    { "input.invalid", "ring-red-400 border-red-400" },
                    { "input.invalid:focus", "ring-red-400 border-red-400" },
                    { "#blazor-error-ui", "bg-yellow-100 text-yellow-900 hidden fixed bottom-0 left-0 w-full p-2" }
                },
            });

            // these css classes are populated via a source generator at build time.
            // they are pulled from all razor files looking for class="..." or cssclass="...".
            // Additionally any strings passed to Monorail.CssClass(string s) are also included.
            var cssClasses = Client.Monorail.CssClassValues();

            // this builds the style sheet given the css classes.
            var css = framework.Process(cssClasses);
            return Results.Text(css, "text/css", Encoding.Default);
        });
    }
}