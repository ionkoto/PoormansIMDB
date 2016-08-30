using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MovieDB.Models
{
    public class Movie
    {
        public Movie()
        {
            this.Date = DateTime.Now;
            this.Comments = new HashSet<Comment>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Author_Id { get; set; }

        [ForeignKey ("Author_Id")]
        public ApplicationUser Author { get; set; }

        public byte[] Image { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        public Int32 Rating { get; set; }

        [Required]
        [StringLength(100)]
        public string Director { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

    }
}