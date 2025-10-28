using Microsoft.AspNetCore.Identity;
using RazorEcom.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    // Navigation properties (không còn virtual)
    public ICollection<AddressBook> AddressBooks { get; set; } = new List<AddressBook>();
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}