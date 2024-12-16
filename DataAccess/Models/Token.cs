using System;
using System.ComponentModel.DataAnnotations;


namespace DataAccess.Models
{
    public class Token
    {
        [Key] public int TokenId { get; set; }

        [Required] public string Email { get; set; }

        [Required] public string TokenValue { get; set; }

        [Required] public DateTime ExpiryDate { get; set; }
    }
}