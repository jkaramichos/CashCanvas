// CashCanvas/Entities/PlaidItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashCanvas.Entities;

public class PlaidAccount
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ItemId { get; set; } = string.Empty;

    [Required]
    public string AccessToken { get; set; } = string.Empty;
    
    [Required]
    public string InstitutionName { get; set; } = string.Empty;

    // Foreign key to ApplicationUser
    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(ApplicationUserId))]
    public virtual ApplicationUser? ApplicationUser { get; set; }
}