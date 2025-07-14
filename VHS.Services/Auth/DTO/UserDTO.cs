using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHS.Services.Auth.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Role { get; set; }
        public DateTime AddedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
