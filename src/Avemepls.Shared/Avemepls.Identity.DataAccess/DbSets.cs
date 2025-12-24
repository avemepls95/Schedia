using Avemepls.Identity.DataAccess.Models;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Identity.DataAccess;

public partial class IdentityDbContext
{
    public DbSet<User> Users { get; set; }
}