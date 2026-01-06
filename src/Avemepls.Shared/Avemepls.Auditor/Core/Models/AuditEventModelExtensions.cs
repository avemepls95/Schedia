using System.Text.Json;

namespace Avemepls.Auditor.Core.Models;

public static class AuditEventModelExtensions
{
    /// <summary>
    /// Get audit event json payload as object
    /// </summary>
    public static TObject? GetJsonPayload<TObject>(this AuditEventModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Payload))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TObject>(model.Payload);
    }
}