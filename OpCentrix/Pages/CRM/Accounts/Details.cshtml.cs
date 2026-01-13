using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Accounts;

public class DetailsModel : PageModel
{
    private readonly ICrmAccountService _accountService;
    private readonly ICrmContactService _contactService;
    private readonly ICrmTaskService _taskService;
    private readonly SchedulerContext _context;

    public DetailsModel(ICrmAccountService accountService, ICrmContactService contactService, 
                       ICrmTaskService taskService, SchedulerContext context)
    {
        _accountService = accountService;
        _contactService = contactService;
        _taskService = taskService;
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public CrmAccount? Account { get; set; }
    public List<CrmContact> Contacts { get; set; } = new();
    public List<CrmTask> Tasks { get; set; } = new();
    public List<User> Users { get; set; } = new();
    
    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Account = await _accountService.GetByIdAsync(Id, ct);
        if (Account == null) return NotFound();

        // Load related data
        Contacts = await _contactService.GetByAccountIdAsync(Id, ct);
        Tasks = await _taskService.ListAsync(status: null, assignedToUserId: null, accountId: Id, ct);
        
        // Load users for task assignment display
        Users = await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        return Page();
    }

    public async Task<IActionResult> OnGetEditContactModalAsync(int contactId, CancellationToken ct)
    {
        var contact = await _contactService.GetByIdAsync(contactId, ct);
        if (contact == null)
        {
            return BadRequest("Contact not found");
        }

        var modalHtml = $@"
        <div class='modal-header bg-purple-50 border-b'>
            <h5 class='modal-title text-lg font-semibold text-purple-800 flex items-center'>
                <i class='fa-solid fa-user-edit mr-2'></i>
                Edit Contact
            </h5>
            <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button>
        </div>
        <div class='modal-body p-6'>
            <form hx-post='/CRM/Accounts/Details?handler=UpdateContact&id={Id}' hx-include='this' class='space-y-4'>
                <input type='hidden' name='ContactId' value='{contact.Id}' />
                
                <div>
                    <label class='form-label text-sm font-medium text-gray-700'>Name *</label>
                    <input type='text' name='Name' value='{contact.Name}' 
                           class='form-control mt-1 w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500' 
                           required maxlength='100' />
                </div>
                
                <div>
                    <label class='form-label text-sm font-medium text-gray-700'>Title</label>
                    <input type='text' name='Title' value='{contact.Title ?? ""}' 
                           class='form-control mt-1 w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500' 
                           maxlength='100' placeholder='e.g., Operations Manager' />
                </div>
                
                <div>
                    <label class='form-label text-sm font-medium text-gray-700'>Email</label>
                    <input type='email' name='Email' value='{contact.Email ?? ""}' 
                           class='form-control mt-1 w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500' 
                           maxlength='255' placeholder='contact@example.com' />
                </div>
                
                <div>
                    <label class='form-label text-sm font-medium text-gray-700'>Phone</label>
                    <input type='tel' name='Phone' value='{contact.Phone ?? ""}' 
                           class='form-control mt-1 w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500' 
                           maxlength='20' placeholder='(555) 123-4567' />
                </div>
            </form>
        </div>
        <div class='modal-footer bg-gray-50 border-t px-6 py-3'>
            <div class='flex justify-end gap-3'>
                <button type='button' class='btn btn-secondary px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50' data-bs-dismiss='modal'>
                    <i class='fa-solid fa-times mr-2'></i>Cancel
                </button>
                <button type='submit' form='modal-form' class='btn btn-primary px-4 py-2 text-sm font-medium text-white bg-purple-600 border border-transparent rounded-lg hover:bg-purple-700' 
                        onclick='submitEditForm()'>
                    <i class='fa-solid fa-save mr-2'></i>Update Contact
                </button>
            </div>
        </div>
        
        <script>
            function submitEditForm() {{
                const form = document.querySelector('#contactModal form');
                if (form) {{
                    htmx.trigger(form, 'submit');
                }}
            }}
        </script>";

        return Content(modalHtml, "text/html");
    }

    public async Task<IActionResult> OnPostUpdateContactAsync(int contactId, string name, string? title, string? email, string? phone, CancellationToken ct)
    {
        try
        {
            await _contactService.UpdateAsync(contactId, name, email, phone, title, ct);
            
            StatusMessage = "Contact updated successfully!";
            
            return Content(@"
                <script>
                    // Close modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('contactModal'));
                    if (modal) modal.hide();
                    
                    // Show success message
                    const alertDiv = document.createElement('div');
                    alertDiv.className = 'alert alert-success alert-dismissible fade show';
                    alertDiv.innerHTML = `
                        <div class='flex items-center'>
                            <i class='fa-solid fa-check-circle text-green-600 mr-2'></i>
                            <span class='text-green-800 font-medium'>Contact updated successfully!</span>
                            <button type='button' class='btn-close ms-auto' data-bs-dismiss='alert'></button>
                        </div>
                    `;
                    
                    // Add alert to page
                    const container = document.querySelector('.p-6.space-y-6');
                    if (container && container.firstChild) {
                        container.insertBefore(alertDiv, container.firstChild);
                    }
                    
                    // Reload page after short delay to show updated contact
                    setTimeout(() => window.location.reload(), 1000);
                </script>
            ", "text/html");
        }
        catch (Exception ex)
        {
            return Content($@"
                <div class='alert alert-danger mt-3'>
                    <div class='flex items-center'>
                        <i class='fa-solid fa-exclamation-triangle text-red-600 mr-2'></i>
                        <div>
                            <strong>Error:</strong> {ex.Message}
                        </div>
                    </div>
                </div>
            ", "text/html");
        }
    }

    public async Task<IActionResult> OnPostDeleteContactAsync(int contactId, CancellationToken ct)
    {
        try
        {
            var deleted = await _contactService.DeleteAsync(contactId, ct);
            
            if (deleted)
            {
                StatusMessage = "Contact deleted successfully!";
                return RedirectToPage(new { Id });
            }
            else
            {
                StatusMessage = "Contact not found.";
                return RedirectToPage(new { Id });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting contact: {ex.Message}";
            return RedirectToPage(new { Id });
        }
    }
}
