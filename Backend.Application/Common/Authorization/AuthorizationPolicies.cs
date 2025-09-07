using Microsoft.AspNetCore.Authorization;

namespace Backend.Application.Common.Authorization
{
    public static class AuthorizationPolicies
    {
        // Permission-based policies
        public const string CanReadUsers = "CanReadUsers";
        public const string CanCreateUsers = "CanCreateUsers";
        public const string CanUpdateUsers = "CanUpdateUsers";
        public const string CanDeleteUsers = "CanDeleteUsers";
        
        public const string CanReadRoles = "CanReadRoles";
        public const string CanCreateRoles = "CanCreateRoles";
        public const string CanUpdateRoles = "CanUpdateRoles";
        public const string CanDeleteRoles = "CanDeleteRoles";
        
        public const string CanReadPermissions = "CanReadPermissions";
        public const string CanCreatePermissions = "CanCreatePermissions";
        public const string CanUpdatePermissions = "CanUpdatePermissions";
        public const string CanDeletePermissions = "CanDeletePermissions";
        
        public const string CanAssignRoles = "CanAssignRoles";
        public const string CanRevokeRoles = "CanRevokeRoles";
        public const string CanManageRolePermissions = "CanManageRolePermissions";
        
        // Resource-based policies
        public const string CanApproveTransaction = "CanApproveTransaction";
        public const string CanViewOwnTransactions = "CanViewOwnTransactions";
        public const string CanViewBranchTransactions = "CanViewBranchTransactions";
        public const string CanManageOwnProfile = "CanManageOwnProfile";
        
        // Admin policies
        public const string IsAdmin = "IsAdmin";
        public const string IsSuperAdmin = "IsSuperAdmin";
        public const string CanAccessAdminPanel = "CanAccessAdminPanel";
        
        // System policies
        public const string CanViewAuditLogs = "CanViewAuditLogs";
        public const string CanManageSystemSettings = "CanManageSystemSettings";
    }
}