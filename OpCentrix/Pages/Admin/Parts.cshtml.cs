using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Pages.Admin
{
    public class PartsModel : PageModel
    {
        private readonly SchedulerContext _context;

        public PartsModel(SchedulerContext context)
        {
            _context = context;
        }

        public List<Part> Parts { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public string MaterialFilter { get; set; } = string.Empty;
        public bool? ActiveFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        [BindProperty]
        public Part PartToEdit { get; set; } = new();

        public async Task OnGetAsync(string? search, string? material, bool? active, int? page)
        {
            SearchTerm = search ?? string.Empty;
            MaterialFilter = material ?? string.Empty;
            ActiveFilter = active;
            PageNumber = page ?? 1;

            var query = _context.Parts.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(p => p.PartNumber.Contains(SearchTerm) || 
                                        p.Description.Contains(SearchTerm) ||
                                        p.Material.Contains(SearchTerm));
            }

            if (!string.IsNullOrEmpty(MaterialFilter))
            {
                query = query.Where(p => p.Material.Contains(MaterialFilter));
            }

            if (ActiveFilter.HasValue)
            {
                query = query.Where(p => p.IsActive == ActiveFilter.Value);
            }

            TotalCount = await query.CountAsync();
            
            Parts = await query
                .OrderBy(p => p.PartNumber)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == id);
            if (part == null)
                return NotFound();

            var editForm = $@"
                <div class='bg-white rounded-lg p-6 max-w-2xl w-full mx-4'>
                    <h3 class='text-lg font-semibold mb-4'>Edit Part #{part.Id}</h3>
                    <form hx-post='/Admin/Parts?handler=Update' hx-include='this' class='space-y-4'>
                        <input type='hidden' name='PartToEdit.Id' value='{part.Id}' />
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Part Number</label>
                                <input type='text' name='PartToEdit.PartNumber' value='{part.PartNumber}' class='w-full border border-gray-300 rounded-lg p-2' required />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Material</label>
                                <input type='text' name='PartToEdit.Material' value='{part.Material}' class='w-full border border-gray-300 rounded-lg p-2' required />
                            </div>
                        </div>
                        
                        <div>
                            <label class='block text-sm font-medium text-gray-700 mb-1'>Description</label>
                            <input type='text' name='PartToEdit.Description' value='{part.Description}' class='w-full border border-gray-300 rounded-lg p-2' required />
                        </div>
                        
                        <div class='grid grid-cols-3 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Avg Duration</label>
                                <input type='text' name='PartToEdit.AvgDuration' value='{part.AvgDuration}' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Duration (Days)</label>
                                <input type='number' name='PartToEdit.AvgDurationDays' value='{part.AvgDurationDays}' class='w-full border border-gray-300 rounded-lg p-2' min='1' />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Estimated Hours</label>
                                <input type='number' name='PartToEdit.EstimatedHours' value='{part.EstimatedHours}' step='0.1' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                        </div>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Material Cost Per Unit</label>
                                <input type='number' name='PartToEdit.MaterialCostPerUnit' value='{part.MaterialCostPerUnit}' step='0.01' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Labor Cost Per Hour</label>
                                <input type='number' name='PartToEdit.StandardLaborCostPerHour' value='{part.StandardLaborCostPerHour}' step='0.01' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                        </div>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Setup Cost</label>
                                <input type='number' name='PartToEdit.SetupCost' value='{part.SetupCost}' step='0.01' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                            
                            <div class='flex items-center pt-6'>
                                <label class='flex items-center'>
                                    <input type='checkbox' name='PartToEdit.IsActive' value='true' {(part.IsActive ? "checked" : "")} class='rounded border-gray-300 text-blue-600 focus:ring-blue-500' />
                                    <span class='ml-2 text-sm text-gray-700'>Active</span>
                                </label>
                            </div>
                        </div>
                        
                        <div class='flex justify-end space-x-3 pt-4'>
                            <button type='button' onclick='hideModal()' class='px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50'>
                                Cancel
                            </button>
                            <button type='submit' class='px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700'>
                                Update Part
                            </button>
                        </div>
                    </form>
                </div>";

            return Content(editForm, "text/html");
        }

        public async Task<IActionResult> OnGetCreateAsync()
        {
            var createForm = @"
                <div class='bg-white rounded-lg p-6 max-w-2xl w-full mx-4'>
                    <h3 class='text-lg font-semibold mb-4'>Create New Part</h3>
                    <form hx-post='/Admin/Parts?handler=Create' hx-include='this' class='space-y-4'>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Part Number</label>
                                <input type='text' name='PartToEdit.PartNumber' class='w-full border border-gray-300 rounded-lg p-2' required />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Material</label>
                                <input type='text' name='PartToEdit.Material' class='w-full border border-gray-300 rounded-lg p-2' required />
                            </div>
                        </div>
                        
                        <div>
                            <label class='block text-sm font-medium text-gray-700 mb-1'>Description</label>
                            <input type='text' name='PartToEdit.Description' class='w-full border border-gray-300 rounded-lg p-2' required />
                        </div>
                        
                        <div class='grid grid-cols-3 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Avg Duration</label>
                                <input type='text' name='PartToEdit.AvgDuration' value='8h 0m' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Duration (Days)</label>
                                <input type='number' name='PartToEdit.AvgDurationDays' value='1' class='w-full border border-gray-300 rounded-lg p-2' min='1' />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Estimated Hours</label>
                                <input type='number' name='PartToEdit.EstimatedHours' value='8' step='0.1' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                        </div>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Material Cost Per Unit</label>
                                <input type='number' name='PartToEdit.MaterialCostPerUnit' value='0' step='0.01' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Labor Cost Per Hour</label>
                                <input type='number' name='PartToEdit.StandardLaborCostPerHour' value='75' step='0.01' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                        </div>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Setup Cost</label>
                                <input type='number' name='PartToEdit.SetupCost' value='150' step='0.01' class='w-full border border-gray-300 rounded-lg p-2' />
                            </div>
                            
                            <div class='flex items-center pt-6'>
                                <label class='flex items-center'>
                                    <input type='checkbox' name='PartToEdit.IsActive' value='true' checked class='rounded border-gray-300 text-blue-600 focus:ring-blue-500' />
                                    <span class='ml-2 text-sm text-gray-700'>Active</span>
                                </label>
                            </div>
                        </div>
                        
                        <div class='flex justify-end space-x-3 pt-4'>
                            <button type='button' onclick='hideModal()' class='px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50'>
                                Cancel
                            </button>
                            <button type='submit' class='px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-lg hover:bg-green-700'>
                                Create Part
                            </button>
                        </div>
                    </form>
                </div>";

            return Content(createForm, "text/html");
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            try
            {
                var existingPart = await _context.Parts.FindAsync(PartToEdit.Id);
                if (existingPart == null)
                    return NotFound();

                // Update properties
                existingPart.PartNumber = PartToEdit.PartNumber;
                existingPart.Description = PartToEdit.Description;
                existingPart.Material = PartToEdit.Material;
                existingPart.AvgDuration = PartToEdit.AvgDuration;
                existingPart.AvgDurationDays = PartToEdit.AvgDurationDays;
                existingPart.EstimatedHours = PartToEdit.EstimatedHours;
                existingPart.MaterialCostPerUnit = PartToEdit.MaterialCostPerUnit;
                existingPart.StandardLaborCostPerHour = PartToEdit.StandardLaborCostPerHour;
                existingPart.SetupCost = PartToEdit.SetupCost;
                existingPart.IsActive = PartToEdit.IsActive;
                existingPart.LastModifiedDate = DateTime.UtcNow;
                existingPart.LastModifiedBy = "Admin";

                await _context.SaveChangesAsync();

                return Content("<script>hideModal(); showNotification('Part updated successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<script>showNotification('Error updating part: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                PartToEdit.CreatedDate = DateTime.UtcNow;
                PartToEdit.LastModifiedDate = DateTime.UtcNow;
                PartToEdit.CreatedBy = "Admin";
                PartToEdit.LastModifiedBy = "Admin";

                _context.Parts.Add(PartToEdit);
                await _context.SaveChangesAsync();

                return Content("<script>hideModal(); showNotification('Part created successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<script>showNotification('Error creating part: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var part = await _context.Parts.FindAsync(id);
                if (part == null)
                    return NotFound();

                // Check if part is used in any jobs
                var jobsUsingPart = await _context.Jobs.CountAsync(j => j.PartId == id);
                if (jobsUsingPart > 0)
                {
                    return Content($"<script>showNotification('Cannot delete part: {jobsUsingPart} jobs are using this part. Deactivate it instead.', 'error');</script>", "text/html");
                }

                _context.Parts.Remove(part);
                await _context.SaveChangesAsync();

                return Content("<script>showNotification('Part deleted successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<script>showNotification('Error deleting part: {ex.Message}', 'error');</script>", "text/html");
            }
        }
    }
}