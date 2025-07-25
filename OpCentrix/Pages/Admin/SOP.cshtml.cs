using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class SOPModel : PageModel
    {
        public string SOPCategory { get; set; } = "overview";
        public string SOPSection { get; set; } = "general";

        public void OnGet(string? category = null, string? section = null)
        {
            SOPCategory = category ?? "overview";
            SOPSection = section ?? "general";
        }

        public IActionResult OnGetGetSOPContent(string category, string section)
        {
            ViewData["Category"] = category;
            ViewData["Section"] = section;
            
            return Partial("_SOPContent");
        }

        public IActionResult OnGetExportAll()
        {
            // Basic implementation for now - return a simple PDF or document
            // In a real implementation, this would generate a comprehensive PDF
            var content = "OpCentrix Standard Operating Procedures\n\n";
            content += "This is a placeholder for the complete SOP export functionality.\n";
            content += "Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            return File(bytes, "text/plain", "OpCentrix_SOPs.txt");
        }
    }
}