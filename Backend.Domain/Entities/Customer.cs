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

        // Contact Information - Using Value Objects
        public Email? Email { get; private set; }
        public PhoneNumber? PhoneNumber { get; private set; }
        public PhoneNumber? MobileNumber { get; private set; }

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

        // Enhanced Computed Properties using Value Objects
        public bool HasBusinessEmail => Email?.IsBusinessEmail() ?? false;
        public bool HasCorporateEmail => Email?.IsCorporateEmail() ?? false;
        public bool HasMobileNumber => MobileNumber?.IsMobile() ?? false;
        public bool HasTehranNumber => MobileNumber?.IsTehranNumber() ?? PhoneNumber?.IsTehranNumber() ?? false;
        public bool HasCompleteAddress => PrimaryAddress?.IsComplete ?? false;

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

        public static Customer Create(string firstName, string lastName, Email email, PhoneNumber phoneNumber, DateTime? dateOfBirth = null, Address? address = null, string? createdBy = null)
        {
            var customer = new Customer(Guid.NewGuid(), null, firstName, lastName, createdBy);
            
            // Set contact information
            customer.Email = email;
            customer.PhoneNumber = phoneNumber;
            customer.DateOfBirth = dateOfBirth;
            customer.PrimaryAddress = address;
            
            customer.AddDomainEvent(new CustomerRegisteredEvent(customer.Id, null));
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

        // Contact Information Methods - Using Value Objects
        public void SetEmail(string? emailValue, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            Email = !string.IsNullOrWhiteSpace(emailValue) ? Email.Create(emailValue) : null;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetEmail(Email? email, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            Email = email;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetPhoneNumber(string? phoneNumberValue, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            PhoneNumber = !string.IsNullOrWhiteSpace(phoneNumberValue) ? PhoneNumber.Create(phoneNumberValue) : null;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetPhoneNumber(PhoneNumber? phoneNumber, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            PhoneNumber = phoneNumber;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetMobileNumber(string? mobileNumberValue, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            MobileNumber = !string.IsNullOrWhiteSpace(mobileNumberValue) ? PhoneNumber.Create(mobileNumberValue) : null;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetMobileNumber(PhoneNumber? mobileNumber, string updatedBy)
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

        // Enhanced Business Logic Methods using Value Objects
        public bool CanPlaceOrder()
        {
            return IsActive && IsAdult && Email != null;
        }

        public bool HasCompleteProfile()
        {
            return Email != null && MobileNumber != null && HasCompleteAddress;
        }

        public bool CanAccessPremiumFeatures()
        {
            return IsActive && IsPremium;
        }

        public bool IsBusinessCustomer()
        {
            return HasBusinessEmail || HasCorporateEmail || !string.IsNullOrWhiteSpace(CompanyName);
        }

        public bool IsLocalCustomer()
        {
            return HasTehranNumber || (PrimaryAddress?.IsInSameCountry(Address.Create("Iran", "Tehran", "Tehran", "District 1", "Test Street", "12345")) ?? false);
        }

        public bool HasValidContactInfo()
        {
            return Email != null && (MobileNumber != null || PhoneNumber != null);
        }

        public string GetContactPreference()
        {
            if (MobileNumber != null) return "Mobile";
            if (PhoneNumber != null) return "Phone";
            if (Email != null) return "Email";
            return "None";
        }

        public bool CanReceiveSMS()
        {
            return MobileNumber != null && MobileNumber.IsMobile();
        }

        public bool CanReceiveEmail()
        {
            return Email != null && !Email.IsDisposableEmail();
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

            // Email validation is now handled by Email Value Object
            // PhoneNumber validation is now handled by PhoneNumber Value Object
        }
    }
} 