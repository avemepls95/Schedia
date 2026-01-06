using Microsoft.AspNetCore.Builder;

namespace Avemepls.Mvc;

public static class VersionHeaderEmbedder
{
    public static void UseVersionHeaderEmbedder(this IApplicationBuilder app)
    {
        app.Use((context, next) =>
        {
            if (!string.IsNullOrEmpty(_buildDate.Value))
            {
                context.Response.Headers["x-build-date"] = _buildDate.Value;
            }

            if (!string.IsNullOrEmpty(_commitSha.Value))
            {
                context.Response.Headers["x-commit-sha"] = _commitSha.Value;
            }

            return next.Invoke();
        });
    }

    private static readonly Lazy<string?> _buildDate = new(Environment.GetEnvironmentVariable("BUILD_DATE"));
    private static readonly Lazy<string?> _commitSha = new(Environment.GetEnvironmentVariable("COMMIT_SHA"));
}