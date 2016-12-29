using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Librarian.Core.Models
{
    public class Shelf
    {
        public Shelf()
        {
            Capacity = 20;
        }

        public int Id { get; set; }
        
        public int Number { get; set; }

        [Required]
        public string Description { get; set; }
        
        [Required]
        public int Capacity { get; set; }

        public List<Book> Books { get; set; }

        public string DisplayText => $"{Number} ({Description})";
    }
}
