using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentLite.Api.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string AccountNumber { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public int Version { get; set; }
    }
}