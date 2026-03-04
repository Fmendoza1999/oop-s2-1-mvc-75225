using System.ComponentModel.DataAnnotations;

namespace Library.MVC.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Isbn { get; set; } = "";
        public string Category { get; set; } = "";
        public bool IsAvailable { get; set; } = true;
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}