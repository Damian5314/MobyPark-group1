using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace v2.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Users in this company
        public List<UserProfile> Users { get; set; } = new();
    }
}