using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace AcxiomCRM.Models;

public class AppUser : IdentityUser { [StringLength(100)] public string FullName { get; set; } = ""; public bool IsActive { get; set; } = true; }

public class Customer {
  public int CustomerId { get; set; }
  [StringLength(20)] public string CustomerCode { get; set; } = "";
  [Required, StringLength(100)] public string CustomerName { get; set; }
  [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(150)] public string Email { get; set; }
  [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid phone number.")] public string Phone { get; set; }
  [StringLength(100)] public string CompanyName { get; set; }
  [StringLength(250)] public string Address { get; set; }
  [StringLength(60)] public string City { get; set; }
  [StringLength(60)] public string State { get; set; }
  public string Status { get; set; } = "Active";
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public string CreatedBy { get; set; }
}

public class Lead {
  public int LeadId { get; set; }
  [Required, StringLength(100)] public string LeadName { get; set; }
  [EmailAddress, StringLength(150)] public string Email { get; set; }
  [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid phone number.")] public string Phone { get; set; }
  public string CompanyName { get; set; }
  public string Source { get; set; }
  [Required] public string Status { get; set; } = "New";   // New, Contacted, Qualified, Unqualified, Converted, Lost
  [Range(0, 100000000)] public decimal ExpectedValue { get; set; }
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public string AssignedTo { get; set; }
}

public class Opportunity : IValidatableObject {
  public int OpportunityId { get; set; }
  [Required, StringLength(120)] public string OpportunityName { get; set; }
  public int CustomerId { get; set; }
  public Customer Customer { get; set; }
  [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")] public int Probability { get; set; }
  public decimal Amount { get; set; }
  [Required] public string Stage { get; set; } = "Qualification"; // Qualification, Proposal, Negotiation, Won, Lost
  [DataType(DataType.Date)] public DateTime ExpectedCloseDate { get; set; } = DateTime.Today.AddDays(30);
  public string Status { get; set; } = "Open";
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public string AssignedTo { get; set; }
  public IEnumerable<ValidationResult> Validate(ValidationContext c) {
    if (Status == "Open") {
      if (Amount <= 0) yield return new("Opportunity Amount must be greater than 0.", new[] { nameof(Amount) });
      if (ExpectedCloseDate.Date < DateTime.Today) yield return new("Expected Close Date cannot be in the past.", new[] { nameof(ExpectedCloseDate) });
    }
  }
}

public class FollowUp : IValidatableObject {
  public int FollowUpId { get; set; }
  public int? CustomerId { get; set; }
  public int? LeadId { get; set; }
  [DataType(DataType.Date)] public DateTime FollowUpDate { get; set; } = DateTime.Today;
  public string FollowUpType { get; set; }
  [StringLength(500)] public string Remarks { get; set; }
  public string Status { get; set; } = "Planned";
  public string AssignedTo { get; set; }
  public IEnumerable<ValidationResult> Validate(ValidationContext c) {
    if (Status == "Planned" && FollowUpDate.Date < DateTime.Today)
      yield return new("Follow-up date cannot be earlier than today.", new[] { nameof(FollowUpDate) });
  }
}

public class AuditLog {
  public int AuditLogId { get; set; }
  public string UserId { get; set; }
  public string Action { get; set; }
  public string EntityName { get; set; }
  public string RecordId { get; set; }
  public string OldValue { get; set; }
  public string NewValue { get; set; }
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public string IpAddress { get; set; }
}
