using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace PoorGuys.DataAccess.Context
{
    internal class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions options)
            : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"data source=(local);Initial Catalog=PoorGuys_DEV;integrated security=True;");
            base.OnConfiguring(optionsBuilder);
        }
    }
}