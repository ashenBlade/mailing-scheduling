using Microsoft.EntityFrameworkCore;

namespace MailingScheduler.Database;

public class MailingDbContext: DbContext
{
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Template> Templates => Set<Template>();

    public MailingDbContext(DbContextOptions<MailingDbContext> options)
        : base(options)
    { }
}