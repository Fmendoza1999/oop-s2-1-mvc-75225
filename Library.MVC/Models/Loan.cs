using System.ComponentModel.DataAnnotations;

namespace Library.MVC.Models
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public Book Book { get; set; } = null!;
        public Member Member { get; set; } = null!;
    }
}