using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Librarian.Core.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }
        
        [DisplayName("Shelf")]
        public Shelf Shelf { get; set; }
        [DisplayName("Shelf")]
        public int ShelfId { get; set; }

        [DisplayName("Position")]
        public int Position { get; set; }

        [DisplayName("Age limit")]
        public int? AgeLimit { get; set; }

        [DisplayName("Duration limit")]
        public int? DurationLimit { get; set; }

        public List<Borrowing> Borrowings { get; set; }


        public string DisplayText => $"{Name} – {Title}";

        [DisplayName("Identifier code")]
        public string IdentifierCode => $"{Shelf?.Number}-{Position}";

    }
}
