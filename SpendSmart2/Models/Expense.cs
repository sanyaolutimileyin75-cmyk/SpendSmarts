using SpendSmart2.Models;
using System.ComponentModel.DataAnnotations;

namespace SpendSmart2.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Category { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}    

