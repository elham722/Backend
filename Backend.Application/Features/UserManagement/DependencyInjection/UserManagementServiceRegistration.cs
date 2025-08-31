using Backend.Application.Common.Behaviors;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Features.UserManagement.Commands;
using Backend.Application.Features.UserManagement.Commands.ChangePassword;
using Backend.Application.Features.UserManagement.Commands.CreateUser;
using Backend.Application.Features.UserManagement.Commands.DeleteUser;
using Backend.Application.Features.UserManagement.Commands.Login;
using Backend.Application.Features.UserManagement.Commands.Logout;
using Backend.Application.Features.UserManagement.Commands.MFA;
using Backend.Application.Features.UserManagement.Commands.RefreshToken;
using Backend.Application.Features.UserManagement.Commands.Register;
using Backend.Application.Features.UserManagement.Commands.UpdateUser;
using Backend.Application.Features.UserManagement.Queries;
using Backend.Application.Features.UserManagement.Queries.GetUserById;
using Backend.Application.Features.UserManagement.Queries.GetUserProfile;
using Backend.Application.Features.UserManagement.Queries.GetUsers;
using Backend.Application.Features.UserManagement.Queries.MFA;
using Backend.Application.Mappers.Profiles;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.Features.UserManagement.DependencyInjection;

/// <summary>
/// User Management services registration
/// </summary>
public static class UserManagementServiceRegistration
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection services)
    {
        // AutoMapper profiles - only Application layer profiles
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MfaMappingProfile>();
        });

        // MediatR handlers
        services.AddMediatR(cfg =>
        {
            // Commands
            cfg.RegisterServicesFromAssemblyContaining<CreateUserCommand>();
            cfg.RegisterServicesFromAssemblyContaining<LoginCommand>();
            cfg.RegisterServicesFromAssemblyContaining<RegisterCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateUserCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeleteUserCommand>();
            cfg.RegisterServicesFromAssemblyContaining<ChangePasswordCommand>();
            cfg.RegisterServicesFromAssemblyContaining<LogoutCommand>();
            cfg.RegisterServicesFromAssemblyContaining<RefreshTokenCommand>();

            // MFA Commands
            cfg.RegisterServicesFromAssemblyContaining<SetupMfaCommand>();
            cfg.RegisterServicesFromAssemblyContaining<VerifyMfaCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DisableMfaCommand>();

            // Queries
            cfg.RegisterServicesFromAssemblyContaining<GetUserByIdQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetUsersQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetUserProfileQuery>();

            // MFA Queries
            cfg.RegisterServicesFromAssemblyContaining<GetMfaMethodsQuery>();
        });

        // Validators are already registered in ApplicationServicesRegistration
        // No need to register them again here

        // Pipeline behaviors are already registered in ApplicationServicesRegistration
        // No need to register them again here

        return services;
    }
} 