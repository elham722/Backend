using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Backend.Domain.Common;
using Backend.Domain.Events.Customer;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities
{
    public class Customer : BaseAggregateRoot<Guid>
    {

        // Personal Information
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? MiddleName { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string? Gender { get; private set; }
        public string? NationalCode { get; private set; }
        public string? PassportNumber { get; private set; }
        public Address? PrimaryAddress { get; private set; }


        // Integration with Identity
        public string? ApplicationUserId { get; private set; }


        private Customer() { } // EF / serializer

        private Customer(Guid id, string applicationUserId, string firstName, string lastName)
        {
            Guard.AgainstNullOrEmpty(applicationUserId, nameof(applicationUserId));
            Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
            Guard.AgainstNullOrEmpty(lastName, nameof(lastName));

            Id = id;
            ApplicationUserId = applicationUserId;
            FirstName = firstName;
            LastName = lastName;
        }

        public static Customer Create(string applicationUserId, string firstName, string lastName)
        {
            var c = new Customer(Guid.NewGuid(), applicationUserId, firstName, lastName);
            c.AddDomainEvent(new CustomerRegisteredEvent(c.Id, applicationUserId));
            return c;
        }

        public void ChangeName(string firstName, string lastName)
        {
            Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
            Guard.AgainstNullOrEmpty(lastName, nameof(lastName));

            FirstName = firstName;
            LastName = lastName;
        }

        public void SetPrimaryAddress(Address address)
        {
            Guard.Against(address == null, "Address cannot be null");
            PrimaryAddress = address;
        }
    }
} 