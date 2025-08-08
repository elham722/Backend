using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Common;
using Backend.Domain.Events.Customer;
using Backend.Domain.ValueObjects;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities
{
    public class Customer : BaseAggregateRoot<Guid>
    {
        // Personal Information
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string? MiddleName { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string? Gender { get; private set; }
        public string? NationalCode { get; private set; }
        public string? PassportNumber { get; private set; }
        public Address? PrimaryAddress { get; private set; }

        // Contact Information
        public string? Email { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? MobileNumber { get; private set; }

        // Business Information
        public EntityStatus Status { get; private set; }
        public CustomerStatus CustomerStatus { get; private set; }
        public string? CompanyName { get; private set; }
        public string? TaxId { get; private set; }

        // Integration with Identity
        public string? ApplicationUserId { get; private set; }

        // Computed Properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string DisplayName => !string.IsNullOrWhiteSpace(MiddleName) 
            ? $"{FirstName} {MiddleName} {LastName}" 
            : FullName;
        public int? Age => DateOfBirth?.Year > 0 
            ? DateTime.UtcNow.Year - DateOfBirth.Value.Year 
            : null;
        public bool IsActive => Status == EntityStatus.Active && CustomerStatus == CustomerStatus.Active;
        public bool IsAdult => Age.HasValue && Age.Value >= 18;
        public bool IsVerified => CustomerStatus == CustomerStatus.Verified;
        public bool IsPremium => CustomerStatus == CustomerStatus.Premium;

        private Customer() { } // For EF Core

        private Customer(Guid id, string applicationUserId, string firstName, string lastName, string? createdBy = null)
        {
            ValidateId(id);
            Guard.AgainstNullOrEmpty(applicationUserId, nameof(applicationUserId));
            Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
            Guard.AgainstNullOrEmpty(lastName, nameof(lastName));

            Id = id;
            ApplicationUserId = applicationUserId;
            FirstName = firstName;
            LastName = lastName;
            Status = EntityStatus.Active;
            CustomerStatus = CustomerStatus.Pending;
            
            if (!string.IsNullOrWhiteSpace(createdBy))
                SetCreatedBy(createdBy);
        }

        public static Customer Create(string applicationUserId, string firstName, string lastName, string? createdBy = null)
        {
            var customer = new Customer(Guid.NewGuid(), applicationUserId, firstName, lastName, createdBy);
            customer.AddDomainEvent(new CustomerRegisteredEvent(customer.Id, applicationUserId));
            customer.OnCreated();
            return customer;
        }

        // Personal Information Methods
        public void ChangeName(string firstName, string lastName, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
            Guard.AgainstNullOrEmpty(lastName, nameof(lastName));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            FirstName = firstName;
            LastName = lastName;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetMiddleName(string? middleName, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            MiddleName = middleName;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetDateOfBirth(DateTime? dateOfBirth, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (dateOfBirth.HasValue && dateOfBirth.Value > DateTime.UtcNow)
                throw new ArgumentException("Date of birth cannot be in the future", nameof(dateOfBirth));

            DateOfBirth = dateOfBirth;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetGender(string? gender, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (!string.IsNullOrWhiteSpace(gender) && !IsValidGender(gender))
                throw new ArgumentException("Invalid gender value", nameof(gender));

            Gender = gender;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetNationalCode(string? nationalCode, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (!string.IsNullOrWhiteSpace(nationalCode) && !IsValidNationalCode(nationalCode))
                throw new ArgumentException("Invalid national code format", nameof(nationalCode));

            NationalCode = nationalCode;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetPassportNumber(string? passportNumber, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            PassportNumber = passportNumber;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Contact Information Methods
        public void SetEmail(string? email, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
                throw new ArgumentException("Invalid email format", nameof(email));

            Email = email;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetPhoneNumber(string? phoneNumber, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            PhoneNumber = phoneNumber;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetMobileNumber(string? mobileNumber, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            MobileNumber = mobileNumber;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Address Methods
        public void SetPrimaryAddress(Address? address, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            PrimaryAddress = address;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Business Information Methods
        public void SetCompanyName(string? companyName, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            CompanyName = companyName;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetTaxId(string? taxId, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            TaxId = taxId;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Status Management Methods
        public void Activate(string activatedBy)
        {
            Guard.AgainstNullOrEmpty(activatedBy, nameof(activatedBy));

            if (Status == EntityStatus.Active && CustomerStatus == CustomerStatus.Active)
                return;

            Status = EntityStatus.Active;
            CustomerStatus = CustomerStatus.Active;
            SetUpdatedBy(activatedBy);
            OnUpdated();
        }

        public void Deactivate(string deactivatedBy)
        {
            Guard.AgainstNullOrEmpty(deactivatedBy, nameof(deactivatedBy));

            if (Status == EntityStatus.Inactive)
                return;

            Status = EntityStatus.Inactive;
            CustomerStatus = CustomerStatus.Inactive;
            SetUpdatedBy(deactivatedBy);
            OnUpdated();
        }

        public void Suspend(string suspendedBy, string? reason = null)
        {
            Guard.AgainstNullOrEmpty(suspendedBy, nameof(suspendedBy));

            Status = EntityStatus.Suspended;
            CustomerStatus = CustomerStatus.Suspended;
            SetUpdatedBy(suspendedBy);
            OnUpdated();
        }

        public void Block(string blockedBy, string? reason = null)
        {
            Guard.AgainstNullOrEmpty(blockedBy, nameof(blockedBy));

            CustomerStatus = CustomerStatus.Blocked;
            SetUpdatedBy(blockedBy);
            OnUpdated();
        }

        public void Verify(string verifiedBy)
        {
            Guard.AgainstNullOrEmpty(verifiedBy, nameof(verifiedBy));

            CustomerStatus = CustomerStatus.Verified;
            SetUpdatedBy(verifiedBy);
            OnUpdated();
        }

        public void UpgradeToPremium(string upgradedBy)
        {
            Guard.AgainstNullOrEmpty(upgradedBy, nameof(upgradedBy));

            if (CustomerStatus != CustomerStatus.Verified)
                throw new InvalidOperationException("Only verified customers can be upgraded to premium");

            CustomerStatus = CustomerStatus.Premium;
            SetUpdatedBy(upgradedBy);
            OnUpdated();
        }

        public void DowngradeToRegular(string downgradedBy)
        {
            Guard.AgainstNullOrEmpty(downgradedBy, nameof(downgradedBy));

            CustomerStatus = CustomerStatus.Regular;
            SetUpdatedBy(downgradedBy);
            OnUpdated();
        }

        public void MarkAsDeleted(string deletedBy)
        {
            Guard.AgainstNullOrEmpty(deletedBy, nameof(deletedBy));

            Status = EntityStatus.Deleted;
            CustomerStatus = CustomerStatus.Deleted;
            MarkAsDeleted(deletedBy);
            OnDeleted();
        }

        public void Restore(string restoredBy)
        {
            Guard.AgainstNullOrEmpty(restoredBy, nameof(restoredBy));

            if (Status != EntityStatus.Deleted)
                throw new InvalidOperationException("Only deleted customers can be restored");

            Status = EntityStatus.Active;
            CustomerStatus = CustomerStatus.Pending;
            Restore();
            SetUpdatedBy(restoredBy);
            OnUpdated();
        }

        // Business Logic Methods
        public bool CanPlaceOrder()
        {
            return IsActive && IsAdult && !string.IsNullOrWhiteSpace(Email);
        }

        public bool HasCompleteProfile()
        {
            return !string.IsNullOrWhiteSpace(Email) && 
                   !string.IsNullOrWhiteSpace(MobileNumber) && 
                   PrimaryAddress != null;
        }

        public bool CanAccessPremiumFeatures()
        {
            return IsActive && IsPremium;
        }

        // Validation Methods
        private static bool IsValidGender(string gender)
        {
            return new[] { "Male", "Female", "Other" }.Contains(gender, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsValidNationalCode(string nationalCode)
        {
            return nationalCode.Length == 10 && nationalCode.All(char.IsDigit);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Override methods
        protected override bool ValidateInvariants()
        {
            return !string.IsNullOrWhiteSpace(FirstName) && 
                   !string.IsNullOrWhiteSpace(LastName) && 
                   !string.IsNullOrWhiteSpace(ApplicationUserId);
        }

        protected override void ValidateAggregateState()
        {
            if (DateOfBirth.HasValue && DateOfBirth.Value > DateTime.UtcNow)
                throw new InvalidOperationException("Date of birth cannot be in the future");

            if (!string.IsNullOrWhiteSpace(NationalCode) && !IsValidNationalCode(NationalCode))
                throw new InvalidOperationException("Invalid national code format");

            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
                throw new InvalidOperationException("Invalid email format");
        }
    }
} 