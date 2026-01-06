using Avemepls.Auditor.Core.Models;
using Avemepls.Auditor.DataAccess.Models;

using Mapster;

namespace Avemepls.Auditor.Domain.Mappers;

internal sealed class AuditEventMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<AuditEvent, AuditEventModel>()
            .Map(trg => trg.IpAddress,
                 src => src.IpAddress == null
                     ? null
                     : src.IpAddress.ToString());
    }
}