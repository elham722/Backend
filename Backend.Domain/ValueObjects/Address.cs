using Backend.Domain.Common;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Backend.Domain.ValueObjects
{
    public class Address:BaseValueObject
    {
        public string Province { get; private set; }
        public string City { get; private set; }
        public string District { get; private set; }
        public string Street { get; private set; }
        public string PostalCode { get; private set; }
        public string Details { get; private set; }

        public Address(string street, string city, string postalCode, string country)
        {
            Guard.AgainstNullOrEmpty(street, nameof(street));
            Guard.AgainstNullOrEmpty(city, nameof(city));
            Guard.AgainstNullOrEmpty(postalCode, nameof(postalCode));
            Guard.AgainstNullOrEmpty(country, nameof(country));

            Street = street;
            City = city;
            PostalCode = postalCode;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return PostalCode;
        }

    }
} 