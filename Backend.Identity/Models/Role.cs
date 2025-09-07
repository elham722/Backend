using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Models
{
    public class Role : IdentityRole<string>
    {
        public string Description { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = null!;
        public string? UpdatedBy { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsSystemRole { get; private set; }
        public int Priority { get; private set; }
        public string? Category { get; private set; }

        private Role() { } // For EF Core

        public static Role Create(string name, string description, string? category = null, int priority = 0, bool isSystemRole = false, string createdBy = "System")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Role description cannot be null or empty", nameof(description));

            return new Role
            {
                Name = name,
                NormalizedName = name.ToUpperInvariant(),
                Description = description,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true,
                IsSystemRole = isSystemRole,
                Priority = priority,
                Category = category
            };
        }

        public void Update(string name, string description, string? category = null, int? priority = null, string updatedBy = "System")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Role description cannot be null or empty", nameof(description));

            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

            if (IsSystemRole)
                throw new InvalidOperationException("Cannot update system roles");

            Name = name;
            NormalizedName = name.ToUpperInvariant();
            Description = description;
            Category = category;
            Priority = priority ?? Priority;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void Deactivate(string deactivatedBy)
        {
            if (string.IsNullOrWhiteSpace(deactivatedBy))
                throw new ArgumentException("Deactivated by cannot be null or empty", nameof(deactivatedBy));

            if (IsSystemRole)
                throw new InvalidOperationException("Cannot deactivate system roles");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;
        }

        public void Activate(string activatedBy)
        {
            if (string.IsNullOrWhiteSpace(activatedBy))
                throw new ArgumentException("Activated by cannot be null or empty", nameof(activatedBy));

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;
        }

        public void ChangePriority(int newPriority, string changedBy)
        {
            if (string.IsNullOrWhiteSpace(changedBy))
                throw new ArgumentException("Changed by cannot be null or empty", nameof(changedBy));

            Priority = newPriority;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = changedBy;
        }
    }
}