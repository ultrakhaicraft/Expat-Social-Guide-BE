using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<UserRole> UserRoles { get; set; }

        public Role()
        {
            UserRoles = new HashSet<UserRole>();
        }
    }
}