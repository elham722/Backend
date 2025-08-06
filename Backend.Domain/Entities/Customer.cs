using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Domain.Entities.Common;

namespace Backend.Domain.Entities
{
    public class Customer :BaseEntity
    {

        // Personal Information
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? MiddleName { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string? Gender { get; private set; }
        public string? NationalCode { get; private set; }
        public string? PassportNumber { get; private set; }


        // Integration with Identity
        public string? UserId { get; private set; } 

    }
} 