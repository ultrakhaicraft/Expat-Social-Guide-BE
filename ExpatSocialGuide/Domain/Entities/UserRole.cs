using System;

namespace Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }

        // Navigation
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }

        public UserRole()
        {
            AssignedAt = DateTime.UtcNow;
        }
    }
}