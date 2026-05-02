using System.Text.Json;

using Avemepls.Core.Extensions;

namespace Avemepls.Auditor.Core.Models;

public static class AuditEventModelExtensions
{
    /// <summary>
    /// Get audit event json payload as object
    /// </summary>
    public static TObject? GetJsonPayload<TObject>(this AuditEventModel model)
    {
        if (model.Payload.IsNullOrWhiteSpace())
        {
            return default;
        }

        return JsonSerializer.Deserialize<TObject>(model.Payload);
    }
}