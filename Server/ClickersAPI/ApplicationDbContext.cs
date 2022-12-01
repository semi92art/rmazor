using ClickersAPI.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace ClickersAPI
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Account>          Accounts          { get; set; }
        public DbSet<AccountDataField> AccountDataFields { get; }
        public DbSet<GameDataField>    GameDataFields    { get; }
        public DbSet<BotClient>        BotClients    { get; }

        public ApplicationDbContext([NotNull] DbContextOptions _Options) : base(_Options)
        { }

        protected override void OnModelCreating(ModelBuilder _ModelBuilder)
        {
            base.OnModelCreating(_ModelBuilder);
        }
    }
}
