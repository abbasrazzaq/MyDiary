using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Data
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public required string Username { get; init; }

        [Required]
        public required string PasswordHash { get; init; }
    }
}
