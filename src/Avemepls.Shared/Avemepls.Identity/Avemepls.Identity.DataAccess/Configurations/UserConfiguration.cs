using Avemepls.Core.DataAccess.Configuration;
using Avemepls.Identity.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avemepls.Identity.DataAccess.Configurations;

public class UserConfiguration : BaseConfiguration<User, int>
{
    protected override void ConfigureCore(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.GoogleId)
            .IsUnique()
            .HasFilter("google_id IS NOT NULL");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("email IS NOT NULL");
    }
}