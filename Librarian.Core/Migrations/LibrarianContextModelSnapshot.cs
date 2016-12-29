using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Librarian.Core.Data;

namespace Librarian.Core.Migrations
{
    [DbContext(typeof(LibrarianContext))]
    class LibrarianContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Librarian.Models.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AgeLimit");

                    b.Property<int?>("ClientId");

                    b.Property<int?>("DurationLimit");

                    b.Property<string>("Name");

                    b.Property<int>("Position");

                    b.Property<int>("ShelfId");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("ShelfId");

                    b.ToTable("Book");
                });

            modelBuilder.Entity("Librarian.Models.Borrowing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookId");

                    b.Property<int>("ClientId");

                    b.Property<DateTime>("DateBorrowed");

                    b.Property<DateTime?>("DateReturned");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.HasIndex("ClientId");

                    b.ToTable("Borrowing");
                });

            modelBuilder.Entity("Librarian.Models.Client", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Birthdate");

                    b.Property<bool>("IsUntrustworthy");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Client");
                });

            modelBuilder.Entity("Librarian.Models.Shelf", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Capacity");

                    b.Property<string>("Description");

                    b.Property<int>("Number");

                    b.HasKey("Id");

                    b.ToTable("Shelf");
                });

            modelBuilder.Entity("Librarian.Models.Book", b =>
                {
                    b.HasOne("Librarian.Models.Client", "BorrowedByClient")
                        .WithMany()
                        .HasForeignKey("ClientId");

                    b.HasOne("Librarian.Models.Shelf", "Shelf")
                        .WithMany("Books")
                        .HasForeignKey("ShelfId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Librarian.Models.Borrowing", b =>
                {
                    b.HasOne("Librarian.Models.Book", "Book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Librarian.Models.Client", "Client")
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
