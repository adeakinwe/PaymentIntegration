using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentLite.Api.Models;

public class IdempotencyRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public long PaymentId { get; set; }

    public string Response { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Payment Payment { get; set; } = null!;
}