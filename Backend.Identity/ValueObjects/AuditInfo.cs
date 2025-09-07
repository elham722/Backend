using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Interfaces.Identity;

namespace Backend.Identity.ValueObjects
{
    public class AuditInfo : IAuditInfo
    {
        public string? CreatedBy { get; private set; }
        public string? UpdatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        
        // IAuditInfo interface properties
        public DateTime CreatedAt { get; private set; }
        public string? LastModifiedBy { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }

        private AuditInfo() { } // For EF Core

        public static AuditInfo Create(string? createdBy = null)
        {
            var now = DateTime.UtcNow;
            return new AuditInfo
            {
                CreatedBy = createdBy,
                UpdatedBy = null,
                UpdatedAt = null,
                CreatedAt = now,
                LastModifiedBy = null,
                LastModifiedAt = null
            };
        }

        public AuditInfo Update(string? updatedBy, IDateTimeService dateTimeService)
        {
            if (dateTimeService == null)
                throw new ArgumentNullException(nameof(dateTimeService));

            UpdatedBy = updatedBy;
            UpdatedAt = dateTimeService.UtcNow;
            LastModifiedBy = updatedBy;
            LastModifiedAt = dateTimeService.UtcNow;
            return this;
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
