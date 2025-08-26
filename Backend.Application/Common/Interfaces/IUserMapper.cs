using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Common.Interfaces;

/// <summary>
/// Interface for user mapping operations
/// </summary>
public interface IUserMapper
{
    /// <summary>
    /// Maps a user entity to UserDto
    /// </summary>
    UserDto MapToUserDto(object userEntity, List<string>? roles = null);
    
    /// <summary>
    /// Maps a collection of user entities to UserDto collection
    /// </summary>
    IEnumerable<UserDto> MapToUserDtoCollection(IEnumerable<object> userEntities, bool includeRoles = true);
    
    /// <summary>
    /// Maps CreateUserDto to user entity
    /// </summary>
    object MapToUserEntity(CreateUserDto createUserDto);
    
    /// <summary>
    /// Maps UpdateUserDto to user entity updates
    /// </summary>
    void MapToUserEntity(UpdateUserDto updateUserDto, object userEntity);
} 