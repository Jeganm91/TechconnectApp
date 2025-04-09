using Microsoft.EntityFrameworkCore;

namespace TechConnect.Models
{
    public class TechDBContext : DbContext
    {
        public TechDBContext(DbContextOptions<TechDBContext> options) : base(options)
        {
        }

        //Implement your code here
        public DbSet<Registrant> Registrants { get; set; }
    }
}
