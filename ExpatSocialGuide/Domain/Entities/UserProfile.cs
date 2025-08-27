using System;

namespace Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public Guid UserId { get; set; }

        // Personal Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? DisplayName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        // Contact
        public string? PhoneNumber { get; set; }
        public bool IsPhoneVerified { get; set; } = false;

        // Work Information
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Campus { get; set; }
        public string? OfficeLocation { get; set; }
        public DateTime? JoinedCompanyDate { get; set; }

        // Location
        public string? HomeCountry { get; set; }
        public string? HomeCity { get; set; }
        public string? CurrentCountry { get; set; }
        public string? CurrentCity { get; set; }
        public string? CurrentAddress { get; set; }

        // Profile
        public string? ProfilePictureUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Bio { get; set; }

        // Preferences
        public string PreferredLanguage { get; set; } = "vi";
        public string Timezone { get; set; } = "SE Asia Standard Time";

        // Gamification
        public int Points { get; set; } = 0;
        public int Level { get; set; } = 1;

        // Onboarding
        public bool OnboardingCompleted { get; set; } = false;

        // Navigation
        public virtual User User { get; set; }
    }
}