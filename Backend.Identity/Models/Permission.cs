using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Models
{
    public class Permission
    {
        public string Id { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string Resource { get; private set; } = null!;
        public string Action { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = null!;
        public string? UpdatedBy { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsSystemPermission { get; private set; }
        public string? Category { get; private set; }
        public int Priority { get; private set; }

        private Permission() { } // For EF Core

        public static Permission Create(string name, string resource, string action, string description, string? category = null, int priority = 0, bool isSystemPermission = false, string createdBy = "System")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Permission name cannot be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(resource))
                throw new ArgumentException("Resource cannot be null or empty", nameof(resource));

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Permission description cannot be null or empty", nameof(description));

            return new Permission
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Resource = resource,
                Action = action,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true,
                IsSystemPermission = isSystemPermission,
                Category = category,
                Priority = priority
            };
        }

        public void Update(string name, string resource, string action, string description, string? category = null, int? priority = null, string updatedBy = "System")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Permission name cannot be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(resource))
                throw new ArgumentException("Resource cannot be null or empty", nameof(resource));

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Permission description cannot be null or empty", nameof(description));

            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

            if (IsSystemPermission)
                throw new InvalidOperationException("Cannot update system permissions");

            Name = name;
            Resource = resource;
            Action = action;
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

            if (IsSystemPermission)
                throw new InvalidOperationException("Cannot deactivate system permissions");

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

        public string GetFullName()
        {
            return $"{Resource}:{Action}";
        }

        public bool MatchesResourceAction(string resource, string action)
        {
            return Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) &&
                   Action.Equals(action, StringComparison.OrdinalIgnoreCase);
        }
    }
}