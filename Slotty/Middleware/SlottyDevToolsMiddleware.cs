using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Slotty.Services;
using System.Text;

namespace Slotty.Middleware;

/// <summary>
/// Middleware that automatically injects Slotty development tools (CSS and JavaScript) into HTML responses.
/// </summary>
public class SlottyDevToolsMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlottyDevToolsMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <exception cref="ArgumentNullException">Thrown when next is null.</exception>
    public SlottyDevToolsMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Invokes the middleware to potentially inject Slotty development tools into HTML responses.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Resolve the asset service from the request scope
        var assetService = context.RequestServices.GetRequiredService<ISlottyAssetService>();
        
        // Check if we should inject dev tools
        if (!assetService.ShouldInjectAssets())
        {
            await _next(context);
            return;
        }

        // Only process HTML responses
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Check if this is an HTML response
            if (context.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(responseBody, leaveOpen: true);
                var responseText = await reader.ReadToEndAsync();
                
                var modifiedHtml = InjectDevTools(responseText, assetService);
                var modifiedBytes = Encoding.UTF8.GetBytes(modifiedHtml);
                
                // Update content length if it was set
                if (context.Response.ContentLength.HasValue)
                {
                    context.Response.ContentLength = modifiedBytes.Length;
                }

                context.Response.Body = originalBodyStream;
                await context.Response.Body.WriteAsync(modifiedBytes);
            }
            else
            {
                // Not HTML, just copy the response as-is
                responseBody.Seek(0, SeekOrigin.Begin);
                context.Response.Body = originalBodyStream;
                await responseBody.CopyToAsync(context.Response.Body);
            }
        }
        catch
        {
            // Restore original stream on error
            context.Response.Body = originalBodyStream;
            throw;
        }
    }

    private string InjectDevTools(string html, ISlottyAssetService assetService)
    {
        try
        {
            var cssContent = assetService.GetCssContent();
            var jsContent = assetService.GetJavaScriptContent();

            // Inject CSS before </head>
            var headEndIndex = html.LastIndexOf("</head>", StringComparison.OrdinalIgnoreCase);
            if (headEndIndex >= 0)
            {
                var cssTag = $"    <!-- Slotty Development Tools CSS -->\n    <style>{cssContent}</style>\n";
                html = html.Insert(headEndIndex, cssTag);
            }

            // Inject JS before </body>
            var bodyEndIndex = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (bodyEndIndex >= 0)
            {
                var jsTag = $"    <!-- Slotty Development Tools JavaScript -->\n    <script>{jsContent}</script>\n";
                html = html.Insert(bodyEndIndex, jsTag);
            }

            return html;
        }
        catch
        {
            // If anything goes wrong, return original HTML
            return html;
        }
    }
} 