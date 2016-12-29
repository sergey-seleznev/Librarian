using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Librarian.Core.Models
{
    public class Borrowing
    {
        public int Id { get; set; }

        [DisplayName("Client")]
        public int ClientId { get; set; }
        [DisplayName("Client")]
        public Client Client { get; set; }

        [DisplayName("Book")]
        public int BookId { get; set; }
        [DisplayName("Book")]
        public Book Book { get; set; }

        [DisplayName("Date borrowed")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime DateBorrowed { get; set; }

        [DisplayName("Date returned")]
        public DateTime? DateReturned { get; set; }
        
        public bool? IsOverdue { get; set; }

        public int Duration => (int)Math.Floor(((DateReturned ?? DateTime.Now) - DateBorrowed).TotalDays);
    }
}
