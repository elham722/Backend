using AutoMapper;
using Backend.Application.Common.Interfaces;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Identity.Models;

namespace Backend.Identity.Services;

/// <summary>
/// Implementation of IUserMapper using AutoMapper
/// </summary>
public class UserMapper : IUserMapper
{
    private readonly IMapper _mapper;

    public UserMapper(IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public UserDto MapToUserDto(object userEntity, List<string>? roles = null)
    {
        if (userEntity is not ApplicationUser user)
            throw new ArgumentException("User entity must be of type ApplicationUser", nameof(userEntity));

        var userDto = _mapper.Map<UserDto>(user);
        
        if (roles != null)
        {
            userDto.Roles = roles;
        }

        return userDto;
    }

    public IEnumerable<UserDto> MapToUserDtoCollection(IEnumerable<object> userEntities, bool includeRoles = true)
    {
        var users = userEntities.Cast<ApplicationUser>();
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        
        // Note: Roles will be populated separately by the service layer
        // since we need to query the database for roles
        
        return userDtos;
    }

    public object MapToUserEntity(CreateUserDto createUserDto)
    {
        return _mapper.Map<ApplicationUser>(createUserDto);
    }

    public void MapToUserEntity(UpdateUserDto updateUserDto, object userEntity)
    {
        if (userEntity is not ApplicationUser user)
            throw new ArgumentException("User entity must be of type ApplicationUser", nameof(userEntity));

        _mapper.Map(updateUserDto, user);
    }
} 