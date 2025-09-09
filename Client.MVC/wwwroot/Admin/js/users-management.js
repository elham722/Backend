/**
 * Users Management JavaScript
 * Handles user management operations in admin panel
 */

$(document).ready(function() {
    // Initialize tooltips
    $('[title]').tooltip();
    
    // Initialize delete confirmation modal
    initializeDeleteModal();
    
    // Initialize search functionality
    initializeSearch();
    
    // Initialize status badges
    initializeStatusBadges();
});

/**
 * Initialize delete confirmation modal
 */
function initializeDeleteModal() {
    window.deleteUser = function(userId, userName) {
        $('#userNameToDelete').text(userName);
        $('#deleteForm').attr('action', '@Url.Action("Delete")/' + userId);
        $('#deleteModal').modal('show');
    };
}

/**
 * Initialize search functionality
 */
function initializeSearch() {
    // Auto-submit search form on Enter key
    $('input[name="searchTerm"]').on('keypress', function(e) {
        if (e.which === 13) {
            $(this).closest('form').submit();
        }
    });
    
    // Clear search button
    $('.search-clear').on('click', function() {
        $('input[name="searchTerm"]').val('');
        $(this).closest('form').submit();
    });
}

/**
 * Initialize status badges with appropriate colors
 */
function initializeStatusBadges() {
    $('.badge').each(function() {
        var badge = $(this);
        var text = badge.text().trim();
        
        // Remove existing classes
        badge.removeClass('badge-success badge-danger badge-warning badge-info badge-secondary badge-light');
        
        // Apply appropriate classes based on text content
        if (text === 'فعال' || text === 'جدید') {
            badge.addClass('badge-success');
        } else if (text === 'غیرفعال' || text === 'قفل شده') {
            badge.addClass('badge-danger');
        } else if (text === '2FA') {
            badge.addClass('badge-info');
        } else if (text.includes('+')) {
            badge.addClass('badge-light');
        } else {
            badge.addClass('badge-secondary');
        }
    });
}

/**
 * Show success message
 */
function showSuccessMessage(message) {
    if (typeof toastr !== 'undefined') {
        toastr.success(message);
    } else {
        alert(message);
    }
}

/**
 * Show error message
 */
function showErrorMessage(message) {
    if (typeof toastr !== 'undefined') {
        toastr.error(message);
    } else {
        alert(message);
    }
}

/**
 * Confirm action with custom message
 */
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

/**
 * Toggle user status
 */
function toggleUserStatus(userId, currentStatus) {
    var action = currentStatus ? 'غیرفعال کردن' : 'فعال کردن';
    var message = 'آیا مطمئن هستید که می‌خواهید این کاربر را ' + action + ' کنید؟';
    
    confirmAction(message, function() {
        // Implement status toggle logic here
        console.log('Toggling status for user:', userId);
    });
}

/**
 * Refresh users table
 */
function refreshUsersTable() {
    location.reload();
}

/**
 * Export users data
 */
function exportUsers() {
    // Implement export functionality
    console.log('Exporting users data...');
}

/**
 * Bulk actions
 */
function performBulkAction(action) {
    var selectedUsers = $('input[name="selectedUsers"]:checked');
    
    if (selectedUsers.length === 0) {
        showErrorMessage('لطفاً حداقل یک کاربر را انتخاب کنید');
        return;
    }
    
    var message = 'آیا مطمئن هستید که می‌خواهید این عملیات را روی ' + selectedUsers.length + ' کاربر انجام دهید؟';
    
    confirmAction(message, function() {
        // Implement bulk action logic here
        console.log('Performing bulk action:', action, 'on', selectedUsers.length, 'users');
    });
}

/**
 * Initialize data table features
 */
function initializeDataTable() {
    if (typeof $.fn.DataTable !== 'undefined') {
        $('.table').DataTable({
            "language": {
                "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Persian.json"
            },
            "pageLength": 25,
            "responsive": true,
            "order": [[5, "desc"]], // Sort by creation date
            "columnDefs": [
                { "orderable": false, "targets": [6] } // Disable sorting on actions column
            ]
        });
    }
}

/**
 * Initialize advanced filters
 */
function initializeAdvancedFilters() {
    // Role filter
    $('#roleFilter').on('change', function() {
        applyFilters();
    });
    
    // Status filter
    $('#statusFilter').on('change', function() {
        applyFilters();
    });
    
    // Date range filter
    $('#dateRangeFilter').on('change', function() {
        applyFilters();
    });
}

/**
 * Apply all active filters
 */
function applyFilters() {
    var filters = {
        searchTerm: $('input[name="searchTerm"]').val(),
        role: $('#roleFilter').val(),
        status: $('#statusFilter').val(),
        dateRange: $('#dateRangeFilter').val()
    };
    
    // Build URL with filters
    var url = '@Url.Action("Index")';
    var params = [];
    
    for (var key in filters) {
        if (filters[key] && filters[key] !== '') {
            params.push(key + '=' + encodeURIComponent(filters[key]));
        }
    }
    
    if (params.length > 0) {
        url += '?' + params.join('&');
    }
    
    window.location.href = url;
}

/**
 * Clear all filters
 */
function clearFilters() {
    $('input[name="searchTerm"]').val('');
    $('#roleFilter').val('');
    $('#statusFilter').val('');
    $('#dateRangeFilter').val('');
    applyFilters();
}

// Initialize everything when document is ready
$(document).ready(function() {
    initializeDataTable();
    initializeAdvancedFilters();
});