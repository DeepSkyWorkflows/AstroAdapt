using AstroAdapt.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroAdapt.Data
{
    /// <summary>
    /// Data access for the astro adapter.
    /// </summary>
    public class AstroContext : DbContext
    {        
        /// <summary>
        /// Creates a new instance of the <see cref="AstroContext"/>.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        public AstroContext(DbContextOptions<AstroContext> options):
            base(options)
        {   

        }

        /// <summary>
        /// Gets or sets the manufacturers list.
        /// </summary>
        public DbSet<Manufacturer> Manufacturers { get; set; } = null!;

        /// <summary>
        /// Gets or sets the available components.
        /// </summary>
        public DbSet<Component> Components { get; set; } = null!;

        /// <summary>
        /// Gets or sets the saved solutions.
        /// </summary>
        public DbSet<SavedSolution> Solutions { get; set; } = null!;

        /// <summary>
        /// Model configuration.
        /// </summary>
        /// <param name="modelBuilder">The <seealso cref="ModelBuilder"/>.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Manufacturer>()
                .HasMany(m => m.Components)
                .WithOne(c => c.Manufacturer);

            modelBuilder.Entity<SavedSolution>()
                .OwnsMany(s => s.Items);
            modelBuilder.Entity<SavedSolution>()
                .OwnsOne(s => s.Target);
            modelBuilder.Entity<SavedSolution>()
                .OwnsOne(s => s.Sensor);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
