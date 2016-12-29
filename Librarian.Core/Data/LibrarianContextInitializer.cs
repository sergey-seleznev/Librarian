using System.Collections.Generic;
using System.Linq;
using Librarian.Core.Models;
using System;

namespace Librarian.Core.Data
{
    public static class LibrarianContextInitializer
    {
        public static void Initialize(LibrarianContext context)
        {
            context.Database.EnsureCreated();

            // check if database has already been seeded
            if (context.Clients.Any()) return;

            // Clients
            context.Clients.AddRange(
                new Client { Name = "Sergey Seleznev", Birthdate = new DateTime(1990, 09, 08) },
                new Client { Name = "Emily Jackson", Birthdate = new DateTime(2013, 12, 31) },
                new Client { Name = "Vasily Pupkine", Birthdate = new DateTime(1984, 01, 12) });
            context.SaveChanges();

            // Shelves and Books
            context.Shelves.AddRange(
                new Shelf {
                    Number = 1,
                    Description = "Computer Science",
                    Capacity = 20,
                    Books = new List<Book>
                    {
                        new Book
                        {
                            Name = "Andrew Troelsen",
                            Title = "C# 6.0 and the .NET 4.6 Framework",
                            Position = 1,
                            DurationLimit = 30
                        },
                        new Book
                        {
                            Name = "Jon Skeet",
                            Title = "C# in Depth",
                            Position = 2
                        },
                        new Book
                        {
                            Name = "Adam Freeman",
                            Title = "Pro ASP.NET MVC 5",
                            Position = 3,
                            AgeLimit = 18,
                            DurationLimit = 7
                        },
                        new Book
                        {
                            Name = "Steven Sanderson",
                            Title = "Pro ASP.NET MVC Framework",
                            Position = 10,
                            AgeLimit = 18
                        },
                        new Book
                        {
                            Name = "Jonathan S. Harbour",
                            Title = "Visual C# Game Programming for Teens",
                            Position = 14,
                            AgeLimit = 12,
                            DurationLimit = 14
                        }
                    }
                },
                new Shelf
                {
                    Number = 2,
                    Description = "User Experience Design",
                    Capacity = 20,
                    Books = new List<Book>
                    {
                        new Book
                        {
                            Name = "Steve Krug",
                            Title = "Don't Make Me Think, Revisited",
                            Position = 1,
                            DurationLimit = 7
                        },
                        new Book
                        {
                            Name = "Don Norman",
                            Title = "The Design of Everyday Things",
                            Position = 2,
                            AgeLimit = 12
                        },
                        new Book
                        {
                            Name = "Jenifer Tidwell",
                            Title = "Designing Interfaces",
                            Position = 3
                        }
                    }
                }
            );
            context.SaveChanges();
        }
    }
}
