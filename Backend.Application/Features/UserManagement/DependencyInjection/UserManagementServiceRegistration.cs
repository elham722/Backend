using Backend.Application.Common.DTOs;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.Commands.ActivateUser;
using Backend.Application.Features.UserManagement.Commands.AssignRoles;
using Backend.Application.Features.UserManagement.Commands.ChangePassword;
using Backend.Application.Features.UserManagement.Commands.CreateUser;
using Backend.Application.Features.UserManagement.Commands.DeactivateUser;
using Backend.Application.Features.UserManagement.Commands.DeleteUser;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.Commands.Logout;
using Backend.Application.Features.UserManagement.Commands.Register;
using Backend.Application.Features.UserManagement.Commands.UpdateUser;
using Backend.Application.Features.UserManagement.DTOs;

using Backend.Application.Features.UserManagement.Queries.GetUserById;
using Backend.Application.Features.UserManagement.Queries.GetUserProfile;
using Backend.Application.Features.UserManagement.Queries.GetUsers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.Features.UserManagement.DependencyInjection;

/// <summary>
/// Dependency injection registration for User Management module
/// </summary>
public static class UserManagementServiceRegistration
{
    /// <summary>
    /// Registers all User Management services
    /// </summary>
    public static IServiceCollection AddUserManagementServices(this IServiceCollection services)
    {
        // Register Command Handlers
        services.AddTransient<IRequestHandler<CreateUserCommand, Result<UserDto>>, CreateUserCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateUserCommand, Result<UserDto>>, UpdateUserCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteUserCommand, Result>, DeleteUserCommandHandler>();
        services.AddTransient<IRequestHandler<LoginCommand, Result<AuthResultDto>>, LoginCommandHandler>();
        services.AddTransient<IRequestHandler<RegisterCommand, Result<AuthResultDto>>, RegisterCommandHandler>();
        services.AddTransient<IRequestHandler<LogoutCommand, Result<LogoutResultDto>>, LogoutCommandHandler>();
        services.AddTransient<IRequestHandler<ChangePasswordCommand, Result>, ChangePasswordCommandHandler>();
        services.AddTransient<IRequestHandler<ActivateUserCommand, Result>, ActivateUserCommandHandler>();
        services.AddTransient<IRequestHandler<DeactivateUserCommand, Result>, DeactivateUserCommandHandler>();
        services.AddTransient<IRequestHandler<AssignRolesCommand, Result>, AssignRolesCommandHandler>();

        // Register Query Handlers
        services.AddTransient<IRequestHandler<GetUserByIdQuery, Result<UserDto>>, GetUserByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetUsersQuery, Result<PaginationResponse<UserDto>>>, GetUsersQueryHandler>();
        services.AddTransient<IRequestHandler<GetUserProfileQuery, Result<UserDto>>, GetUserProfileQueryHandler>();

        // Register Validators
        services.AddTransient<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddTransient<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();
        services.AddTransient<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddTransient<IValidator<RegisterCommand>, RegisterCommandValidator>();
        services.AddTransient<IValidator<LogoutCommand>, LogoutCommandValidator>();
        services.AddTransient<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();

        return services;
    }
} 