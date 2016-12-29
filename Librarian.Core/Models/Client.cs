using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Librarian.Core.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Birth date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime Birthdate { get; set; }

        [DisplayName("Is untrustworthy")]
        public bool IsUntrustworthy { get; set; }

        public List<Borrowing> Borrowings { get; set; }

        public int Age
        {
            get
            {
                int age = DateTime.Today.Year - Birthdate.Year;
                if (Birthdate > DateTime.Today.AddYears(-age)) age--;

                return age;
            }
        }
    }
}
