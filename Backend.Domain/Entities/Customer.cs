using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Common;
using Backend.Domain.Events.Customer;
using Backend.Domain.ValueObjects;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects.Common;

namespace Backend.Domain.Entities
{
    public class Customer : BaseAggregateRoot<Guid>
    {
        // Personal Information
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string? MiddleName { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public Gender? Gender { get; private set; } // Changed to enum
        public NationalCode? NationalCode { get; private set; } // Changed to Value Object
        public string? PassportNumber { get; private set; }
        public Address? PrimaryAddress { get; private set; }

        // Contact Information
        public Email? Email { get; private set; }
        public PhoneNumber? PhoneNumber { get; private set; }
        public PhoneNumber? MobileNumber { get; private set; }

        // Business Information
        public CustomerStatus CustomerStatus { get; private set; } // Only CustomerStatus
        public string? CompanyName { get; private set; }
        public string? TaxId { get; private set; }

        // Integration with Identity
        public string? ApplicationUserId { get; private set; } // Nullable for non-auth scenarios

        // Computed Properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string DisplayName => !string.IsNullOrWhiteSpace(MiddleName) ? $"{FirstName} {MiddleName} {LastName}" : FullName;
        public int? Age => DateOfBirth?.Year > 0 ? DateTime.UtcNow.Year - DateOfBirth.Value.Year : null; // TODO: Inject ISystemClock
        public bool IsActive => CustomerStatus == CustomerStatus.Active;
        public bool IsAdult => Age.HasValue && Age.Value >= 18;
        public bool IsVerified => CustomerStatus == CustomerStatus.Verified;
        public bool IsPremium => CustomerStatus == CustomerStatus.Premium;
        public bool HasBusinessEmail => Email?.IsBusinessEmail() ?? false;
        public bool HasCorporateEmail => Email?.IsCorporateEmail() ?? false;
        public bool HasMobileNumber => MobileNumber?.IsMobile() ?? false;
        public bool HasTehranNumber => MobileNumber?.IsTehranNumber() ?? PhoneNumber?.IsTehranNumber() ?? false;
        public bool HasCompleteAddress => PrimaryAddress?.IsComplete ?? false;

        private Customer() { } // For EF Core

        private Customer(Guid id, string? applicationUserId, string firstName, string lastName, string? createdBy)
        {
            ValidateId(id);
            Id = id;
            ApplicationUserId = applicationUserId; // Nullable
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            CustomerStatus = CustomerStatus.Pending;
            if (!string.IsNullOrWhiteSpace(createdBy))
                SetCreatedBy(createdBy);
        }

        public static Customer CreateWithIdentity(string applicationUserId, string firstName, string lastName, string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(applicationUserId))
                throw new ArgumentNullException(nameof(applicationUserId));
            var customer = new Customer(Guid.NewGuid(), applicationUserId, firstName, lastName, createdBy);
            customer.AddDomainEvent(new CustomerRegisteredEvent(customer.Id, applicationUserId));
            customer.OnCreated();
            return customer;
        }

        public static Customer CreateWithoutIdentity(string firstName, string lastName, Email email, PhoneNumber? phoneNumber = null, DateTime? dateOfBirth = null, Address? address = null, string? createdBy = null)
        {
            var customer = new Customer(Guid.NewGuid(), null, firstName, lastName, createdBy);
            customer.Email = email ?? throw new ArgumentNullException(nameof(email));
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
            var previousFullName = FullName; // قبل از تغییر ذخیره کن
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            SetUpdatedBy(updatedBy);
            AddDomainEvent(new CustomerNameChangedEvent(Id, FullName, previousFullName));
            OnUpdated();
        }

        public void SetMiddleName(string? middleName, string updatedBy)
        {
            MiddleName = middleName;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetDateOfBirth(DateTime? dateOfBirth, string updatedBy)
        {
            if (dateOfBirth.HasValue && dateOfBirth.Value > DateTime.UtcNow)
                throw new ArgumentException("Date of birth cannot be in the future", nameof(dateOfBirth));
            DateOfBirth = dateOfBirth;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetGender(Gender? gender, string updatedBy)
        {
            Gender = gender;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetNationalCode(NationalCode? nationalCode, string updatedBy)
        {
            NationalCode = nationalCode;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetPassportNumber(string? passportNumber, string updatedBy)
        {
            PassportNumber = passportNumber;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Contact Information Methods
        public void SetEmail(Email? email, string updatedBy)
        {
            Email = email;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetPhoneNumber(PhoneNumber? phoneNumber, string updatedBy)
        {
            PhoneNumber = phoneNumber;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetMobileNumber(PhoneNumber? mobileNumber, string updatedBy)
        {
            MobileNumber = mobileNumber;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetPrimaryAddress(Address? address, string updatedBy)
        {
            PrimaryAddress = address;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Business Information Methods
        public void SetCompanyName(string? companyName, string updatedBy)
        {
            CompanyName = companyName;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void SetTaxId(string? taxId, string updatedBy)
        {
            TaxId = taxId;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Status Management Methods
        public void Activate(string activatedBy)
        {
            if (CustomerStatus == CustomerStatus.Active)
                return;
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Active;
            SetUpdatedBy(activatedBy ?? throw new ArgumentNullException(nameof(activatedBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus));
            OnUpdated();
        }

        public void Deactivate(string deactivatedBy)
        {
            if (CustomerStatus == CustomerStatus.Inactive)
                return;
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Inactive;
            SetUpdatedBy(deactivatedBy ?? throw new ArgumentNullException(nameof(deactivatedBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus));
            OnUpdated();
        }

        public void Suspend(string suspendedBy, string? reason = null)
        {
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Suspended;
            SetUpdatedBy(suspendedBy ?? throw new ArgumentNullException(nameof(suspendedBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus, reason));
            OnUpdated();
        }

        public void Block(string blockedBy, string? reason = null)
        {
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Blocked;
            SetUpdatedBy(blockedBy ?? throw new ArgumentNullException(nameof(blockedBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus, reason));
            OnUpdated();
        }

        public void Verify(string verifiedBy)
        {
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Verified;
            SetUpdatedBy(verifiedBy ?? throw new ArgumentNullException(nameof(verifiedBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus));
            OnUpdated();
        }

        public void UpgradeToPremium(string upgradedBy)
        {
            if (CustomerStatus != CustomerStatus.Verified)
                throw new InvalidOperationException("Only verified customers can be upgraded to premium");
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Premium;
            SetUpdatedBy(upgradedBy ?? throw new ArgumentNullException(nameof(upgradedBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus));
            OnUpdated();
        }

        public void DowngradeToRegular(string downgradedBy)
        {
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Regular;
            SetUpdatedBy(downgradedBy ?? throw new ArgumentNullException(nameof(downgradedBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus));
            OnUpdated();
        }

        public new void MarkAsDeleted(string deletedBy)
        {
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Deleted;
            base.MarkAsDeleted(deletedBy ?? throw new ArgumentNullException(nameof(deletedBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus));
            OnDeleted();
        }

        public new void Restore(string restoredBy)
        {
            if (CustomerStatus != CustomerStatus.Deleted)
                throw new InvalidOperationException("Only deleted customers can be restored");
            var previousStatus = CustomerStatus;
            CustomerStatus = CustomerStatus.Pending;
            base.Restore();
            SetUpdatedBy(restoredBy ?? throw new ArgumentNullException(nameof(restoredBy)));
            AddDomainEvent(new CustomerStatusChangedEvent(Id, CustomerStatus, previousStatus));
            OnUpdated();
        }

        // Business Logic Methods
        public bool CanPlaceOrder() => IsActive && IsAdult && Email != null;
        public bool HasCompleteProfile() => Email != null && MobileNumber != null && HasCompleteAddress;
        public bool CanAccessPremiumFeatures() => IsActive && IsPremium;
        public bool IsBusinessCustomer() => HasBusinessEmail || HasCorporateEmail || !string.IsNullOrWhiteSpace(CompanyName);
        public bool IsLocalCustomer() => HasTehranNumber; // Simplified, move country check to Address
        public bool HasValidContactInfo() => Email != null && (MobileNumber != null || PhoneNumber != null);
        public string GetContactPreference()
        {
            if (MobileNumber != null) return "Mobile";
            if (PhoneNumber != null) return "Phone";
            if (Email != null) return "Email";
            return "None";
        }
        public bool CanReceiveSMS() => MobileNumber != null && MobileNumber.IsMobile();
        public bool CanReceiveEmail() => Email != null && !Email.IsDisposableEmail();

        // Validation
        protected override void ValidateState()
        {
            if (DateOfBirth.HasValue && DateOfBirth.Value > DateTime.UtcNow)
                throw new InvalidOperationException("Date of birth cannot be in the future");
        }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public class NationalCode : BaseValueObject
    {
        public string Value { get; }

        private NationalCode(string value)
        {
            if (!IsValid(value))
                throw new ArgumentException("Invalid national code format", nameof(value));
            Value = value;
        }

        public static NationalCode Create(string value) => new NationalCode(value);

        private static bool IsValid(string value) => value?.Length == 10 && value.All(char.IsDigit);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 