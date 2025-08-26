using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Backend.Domain.ValueObjects.Common;
using Backend.Domain.Common;

namespace Backend.Domain.ValueObjects
{
    public class AvatarUrl : BaseValueObject
    {
        public string Value { get; private set; } = null!;
        public AvatarType Type { get; private set; }
        public string? Provider { get; private set; }
        public string? FileExtension { get; private set; }
        public int? Width { get; private set; }
        public int? Height { get; private set; }

        private AvatarUrl() { } // For EF Core

        public AvatarUrl(string value)
        {
            Guard.AgainstNullOrEmpty(value, nameof(value));

            if (!IsValidAvatarUrl(value))
                throw new ArgumentException("Invalid avatar URL format", nameof(value));

            Value = value.Trim();
            Type = DetermineAvatarType(value);
            Provider = ExtractProvider(value);
            FileExtension = ExtractFileExtension(value);
            ExtractDimensions(value);
        }

        public static AvatarUrl Create(string value)
        {
            return new AvatarUrl(value);
        }

        public static AvatarUrl CreateGravatar(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var emailHash = System.Security.Cryptography.MD5.Create()
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes(email.ToLowerInvariant().Trim()));
            var hashString = BitConverter.ToString(emailHash).Replace("-", "").ToLowerInvariant();

            return new AvatarUrl($"https://www.gravatar.com/avatar/{hashString}?d=mp&s=200");
        }

        public static AvatarUrl CreateDefault()
        {
            return new AvatarUrl("https://via.placeholder.com/200x200/CCCCCC/FFFFFF?text=User");
        }

        public static AvatarUrl CreateInitials(string initials, string backgroundColor = "CCCCCC", string textColor = "FFFFFF")
        {
            if (string.IsNullOrWhiteSpace(initials))
                throw new ArgumentException("Initials cannot be null or empty", nameof(initials));

            if (initials.Length > 3)
                throw new ArgumentException("Initials cannot be longer than 3 characters", nameof(initials));

            var encodedInitials = Uri.EscapeDataString(initials.ToUpper());
            return new AvatarUrl($"https://via.placeholder.com/200x200/{backgroundColor}/{textColor}?text={encodedInitials}");
        }

        // Validation Methods
        private bool IsValidAvatarUrl(string url)
        {
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    return false;

                var uri = new Uri(url);
                var scheme = uri.Scheme.ToLowerInvariant();
                
                // Allow HTTP, HTTPS, and data URLs
                if (scheme != "http" && scheme != "https" && scheme != "data")
                    return false;

                // Check for common image file extensions
                var path = uri.AbsolutePath.ToLowerInvariant();
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp" };
                
                if (scheme != "data" && !validExtensions.Any(ext => path.EndsWith(ext)))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private AvatarType DetermineAvatarType(string url)
        {
            if (url.StartsWith("data:"))
                return AvatarType.DataUrl;

            if (url.Contains("gravatar.com"))
                return AvatarType.Gravatar;

            if (url.Contains("placeholder.com"))
                return AvatarType.Placeholder;

            if (url.Contains("ui-avatars.com"))
                return AvatarType.UiAvatars;

            if (url.Contains("robohash.org"))
                return AvatarType.Robohash;

            return AvatarType.Custom;
        }

        private string? ExtractProvider(string url)
        {
            try
            {
                var uri = new Uri(url);
                var host = uri.Host.ToLowerInvariant();

                return host switch
                {
                    "www.gravatar.com" or "gravatar.com" => "Gravatar",
                    "via.placeholder.com" or "placeholder.com" => "Placeholder",
                    "ui-avatars.com" => "UI Avatars",
                    "robohash.org" => "Robohash",
                    _ => host
                };
            }
            catch
            {
                return null;
            }
        }

        private string? ExtractFileExtension(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;
                var lastDotIndex = path.LastIndexOf('.');
                
                if (lastDotIndex > 0 && lastDotIndex < path.Length - 1)
                {
                    var extension = path.Substring(lastDotIndex + 1).ToLowerInvariant();
                    var validExtensions = new[] { "jpg", "jpeg", "png", "gif", "webp", "svg", "bmp" };
                    
                    if (validExtensions.Contains(extension))
                        return extension;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private void ExtractDimensions(string url)
        {
            try
            {
                var uri = new Uri(url);
                var query = uri.Query;

                // Extract dimensions from query parameters
                var widthMatch = Regex.Match(query, @"[?&]w=(\d+)");
                var heightMatch = Regex.Match(query, @"[?&]h=(\d+)");
                var sizeMatch = Regex.Match(query, @"[?&]s=(\d+)");

                if (widthMatch.Success && int.TryParse(widthMatch.Groups[1].Value, out var width))
                    Width = width;

                if (heightMatch.Success && int.TryParse(heightMatch.Groups[1].Value, out var height))
                    Height = height;

                // If no width/height found, try to extract from path
                if (!Width.HasValue && !Height.HasValue)
                {
                    var pathMatch = Regex.Match(uri.AbsolutePath, @"(\d+)x(\d+)");
                    if (pathMatch.Success)
                    {
                        if (int.TryParse(pathMatch.Groups[1].Value, out var pathWidth))
                            Width = pathWidth;
                        if (int.TryParse(pathMatch.Groups[2].Value, out var pathHeight))
                            Height = pathHeight;
                    }
                }

                // If still no dimensions, try size parameter
                if (!Width.HasValue && !Height.HasValue && sizeMatch.Success)
                {
                    if (int.TryParse(sizeMatch.Groups[1].Value, out var size))
                    {
                        Width = size;
                        Height = size;
                    }
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        // Business Logic Methods
        public bool IsGravatar()
        {
            return Type == AvatarType.Gravatar;
        }

        public bool IsPlaceholder()
        {
            return Type == AvatarType.Placeholder;
        }

        public bool IsDataUrl()
        {
            return Type == AvatarType.DataUrl;
        }

        public bool HasDimensions()
        {
            return Width.HasValue && Height.HasValue;
        }

        public bool IsSquare()
        {
            return Width.HasValue && Height.HasValue && Width.Value == Height.Value;
        }

        public string GetResizedUrl(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive");

            if (Type == AvatarType.Gravatar)
            {
                var separator = Value.Contains('?') ? '&' : '?';
                return $"{Value}{separator}s={Math.Max(width, height)}";
            }

            if (Type == AvatarType.Placeholder)
            {
                return $"https://via.placeholder.com/{width}x{height}/CCCCCC/FFFFFF?text=User";
            }

            // For other types, return original URL
            return Value;
        }

        public string GetSquareUrl(int size)
        {
            return GetResizedUrl(size, size);
        }

        public string GetThumbnailUrl()
        {
            return GetSquareUrl(100);
        }

        public string GetMediumUrl()
        {
            return GetSquareUrl(200);
        }

        public string GetLargeUrl()
        {
            return GetSquareUrl(400);
        }

        // Override methods
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
            yield return Type;
            yield return Provider;
            yield return FileExtension;
            yield return Width;
            yield return Height;
        }

        public override string ToString() => Value;

        public static implicit operator string(AvatarUrl avatarUrl) => avatarUrl.Value;
    }

    public enum AvatarType
    {
        Custom,
        Gravatar,
        Placeholder,
        DataUrl,
        UiAvatars,
        Robohash
    }
} 