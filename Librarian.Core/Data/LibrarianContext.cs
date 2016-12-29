using Librarian.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Librarian.Core.Data
{
    public class LibrarianContext : DbContext
    {
        public LibrarianContext(DbContextOptions<LibrarianContext> options) 
                : base(options) { }

        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // singularize table names
            modelBuilder.Entity<Shelf>().ToTable("Shelf");
            modelBuilder.Entity<Book>().ToTable("Book");
            modelBuilder.Entity<Client>().ToTable("Client");
            modelBuilder.Entity<Borrowing>().ToTable("Borrowing");
        }
    }
}
