using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Identity.ValueObjects
{
    public class AuditInfo
    {
        public string? CreatedBy { get; private set; }
        public string? UpdatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private AuditInfo() { } // For EF Core

        public static AuditInfo Create(string? createdBy = null)
        {
            return new AuditInfo
            {
                CreatedBy = createdBy,
                UpdatedBy = null,
                UpdatedAt = null
            };
        }

        public AuditInfo Update(string? updatedBy)
        {
            return new AuditInfo
            {
                CreatedBy = this.CreatedBy,
                UpdatedBy = updatedBy,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public bool HasBeenModified => UpdatedAt.HasValue;

        public bool WasCreatedBy(string userId)
        {
            return CreatedBy == userId;
        }

        public bool WasLastUpdatedBy(string userId)
        {
            return UpdatedBy == userId;
        }
    }
}
