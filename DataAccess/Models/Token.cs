using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Token
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string TokenValue { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; } 
    }
}
