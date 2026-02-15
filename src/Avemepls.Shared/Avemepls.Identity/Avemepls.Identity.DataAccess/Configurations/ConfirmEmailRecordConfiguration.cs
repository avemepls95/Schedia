using Avemepls.Core.DataAccess.Configuration;
using Avemepls.Identity.DataAccess.Models;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avemepls.Identity.DataAccess.Configurations;

public class ConfirmEmailRecordConfiguration : BaseConfiguration<ConfirmEmailRecord, int>
{
    protected override void ConfigureCore(EntityTypeBuilder<ConfirmEmailRecord> builder)
    {
        base.ConfigureCore(builder);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}