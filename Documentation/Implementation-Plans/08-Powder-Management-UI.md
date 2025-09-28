# Implementation Plan 8: Powder Management UI

**Priority: MEDIUM**  
**Estimated Time: 2 days**  
**Dependencies: Plans 1-7 must be completed first**

## Overview

Create a comprehensive powder inventory management UI that integrates with the existing dashboard. This enhances the existing PrintTracking system with powder stock indicators, inventory management, and usage reporting.

## Current State

**What We Have:**
- ? PowderStock and PowderConsumption tables (from Plan 1)
- ? PowderInventoryService (from Plan 5) 
- ? Basic powder tracking in PrintTracking modals (from Plan 3)
- ? Simple powder stock display in dashboard (from Plan 3)

**What We Need:**
- ? Comprehensive powder inventory management interface
- ? Detailed usage reports and analytics  
- ? Stock alerts and reorder notifications
- ? Admin interface for powder management
- ? Enhanced dashboard powder indicators

## Solution: Integrated Powder Management System

Enhance the existing dashboard and admin interfaces with comprehensive powder management capabilities.

---

## Step-by-Step Implementation

### Step 1: Enhanced Powder Stock Dashboard Component

#### 1A: Update PrintTracking Dashboard with Enhanced Powder Display
**File: `Pages/PrintTracking/Index.cshtml`**

Replace the existing powder stock summary with an enhanced version:

```html
<!-- Enhanced Powder Stock Summary -->
@if (Model.PowderStocks.Any())
{
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header bg-gradient-info text-white d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fas fa-flask me-2"></i>Powder Inventory
                    </h5>
                    <div>
                        <span class="badge bg-light text-dark me-2">
                            @Model.PowderStocks.Count(p => p.IsLowStock) Low Stock
                        </span>
                        <a href="/Admin/PowderInventory" class="btn btn-light btn-sm">
                            <i class="fas fa-cog me-1"></i>Manage
                        </a>
                    </div>
                </div>
                <div class="card-body">
                    <div class="row">
                        @foreach (var powder in Model.PowderStocks)
                        {
                            <div class="col-lg-3 col-md-4 col-sm-6 mb-3">
                                <div class="card h-100 @(powder.IsLowStock ? "border-warning" : "border-light")">
                                    <div class="card-body p-3">
                                        <div class="d-flex align-items-center mb-2">
                                            <i class="fas @powder.StatusIcon me-2 @powder.StatusClass h5 mb-0"></i>
                                            <div class="flex-grow-1">
                                                <h6 class="mb-0 fw-bold text-truncate" title="@powder.MaterialType">
                                                    @powder.MaterialType
                                                </h6>
                                                <small class="text-muted">Current Stock</small>
                                            </div>
                                        </div>
                                        
                                        <div class="text-center">
                                            <div class="h4 @powder.StatusClass mb-1">
                                                @powder.CurrentStock.ToString("F1") kg
                                            </div>
                                            
                                            @if (powder.IsLowStock)
                                            {
                                                <span class="badge bg-warning text-dark">
                                                    <i class="fas fa-exclamation-triangle me-1"></i>
                                                    Low Stock
                                                </span>
                                            }
                                            else
                                            {
                                                <span class="badge bg-success">
                                                    <i class="fas fa-check me-1"></i>
                                                    In Stock
                                                </span>
                                            }
                                        </div>
                                        
                                        <div class="mt-2">
                                            <div class="progress" style="height: 6px;">
                                                @{
                                                    var stockPercent = powder.ReorderPoint > 0 ? 
                                                        Math.Min(100, (powder.CurrentStock / powder.ReorderPoint * 100)) : 100;
                                                    var progressClass = powder.IsLowStock ? "bg-warning" : "bg-success";
                                                }
                                                <div class="progress-bar @progressClass" 
                                                     style="width: @stockPercent%"
                                                     title="@stockPercent.ToString("F0")% of reorder point">
                                                </div>
                                            </div>
                                            <small class="text-muted">
                                                Reorder at @powder.ReorderPoint.ToString("F1") kg
                                            </small>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        }
                    </div>
                    
                    @if (Model.PowderStocks.Any(p => p.IsLowStock))
                    {
                        <div class="alert alert-warning mt-3 mb-0">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            <strong>Action Required:</strong> 
                            @Model.PowderStocks.Count(p => p.IsLowStock) material(s) are running low. 
                            <a href="/Admin/PowderInventory" class="alert-link">Manage inventory ?</a>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
}
```

### Step 2: Create Powder Inventory Management Page

#### 2A: Create Admin Powder Inventory Page
**File: `Pages/Admin/PowderInventory.cshtml`**

```html
@page "/Admin/PowderInventory"
@model OpCentrix.Pages.Admin.PowderInventoryModel
@{
    ViewData["Title"] = "Powder Inventory Management";
    Layout = "~/Pages/Shared/_Layout.cshtml";
}

<div class="container-fluid">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h2 class="mb-0">
                <i class="fas fa-flask me-2 text-primary"></i>
                Powder Inventory Management
            </h2>
            <p class="text-muted mb-0">Manage powder stock levels and track usage</p>
        </div>
        <div>
            <button type="button" class="btn btn-success me-2" onclick="showAddMaterialModal()">
                <i class="fas fa-plus me-1"></i>Add Material
            </button>
            <button type="button" class="btn btn-outline-primary" onclick="showUsageReportModal()">
                <i class="fas fa-chart-bar me-1"></i>Usage Report
            </button>
        </div>
    </div>

    <!-- Low Stock Alerts -->
    @if (Model.PowderStocks.Any(p => p.IsLowStock))
    {
        <div class="row mb-4">
            <div class="col-12">
                <div class="alert alert-warning">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong>Low Stock Alert:</strong> 
                    The following materials are running low:
                    @foreach (var lowStock in Model.PowderStocks.Where(p => p.IsLowStock))
                    {
                        <span class="badge bg-warning text-dark ms-1">@lowStock.MaterialType (@lowStock.CurrentStock.ToString("F1") kg)</span>
                    }
                </div>
            </div>
        </div>
    }

    <!-- Powder Stock Management -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0">
                        <i class="fas fa-warehouse me-2"></i>
                        Powder Stock Levels
                    </h5>
                </div>
                <div class="card-body">
                    @if (Model.PowderStocks.Any())
                    {
                        <div class="table-responsive">
                            <table class="table table-hover">
                                <thead>
                                    <tr>
                                        <th>Material Type</th>
                                        <th class="text-center">Current Stock</th>
                                        <th class="text-center">Reorder Point</th>
                                        <th class="text-center">Status</th>
                                        <th class="text-center">Cost/kg</th>
                                        <th class="text-center">Stock Value</th>
                                        <th class="text-center">Last Restocked</th>
                                        <th class="text-center">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach (var stock in Model.PowderStocks.OrderBy(p => p.MaterialType))
                                    {
                                        <tr class="@(stock.IsLowStock ? "table-warning" : "")">
                                            <td>
                                                <div class="fw-bold">@stock.MaterialType</div>
                                                @if (stock.IsLowStock)
                                                {
                                                    <small class="text-warning">
                                                        <i class="fas fa-exclamation-triangle me-1"></i>Low Stock
                                                    </small>
                                                }
                                            </td>
                                            <td class="text-center">
                                                <span class="fw-bold @stock.StatusClass">
                                                    @stock.CurrentStock.ToString("F1") kg
                                                </span>
                                            </td>
                                            <td class="text-center">@stock.ReorderPoint.ToString("F1") kg</td>
                                            <td class="text-center">
                                                <span class="badge @(stock.IsLowStock ? "bg-warning" : "bg-success")">
                                                    @(stock.IsLowStock ? "Low" : "Good")
                                                </span>
                                            </td>
                                            <td class="text-center">$@stock.CostPerKg.ToString("F2")</td>
                                            <td class="text-center">
                                                <span class="fw-bold text-info">$@stock.StockValue.ToString("F2")</span>
                                            </td>
                                            <td class="text-center">
                                                <small class="text-muted">
                                                    @(stock.LastRestocked?.ToString("MMM dd, yyyy") ?? "Never")
                                                </small>
                                            </td>
                                            <td class="text-center">
                                                <div class="btn-group">
                                                    <button type="button" class="btn btn-sm btn-outline-success" 
                                                            onclick="showRestockModal(@stock.Id, '@stock.MaterialType')">
                                                        <i class="fas fa-plus me-1"></i>Restock
                                                    </button>
                                                    <button type="button" class="btn btn-sm btn-outline-primary"
                                                            onclick="showUpdateStockModal(@stock.Id, '@stock.MaterialType', @stock.CurrentStock)">
                                                        <i class="fas fa-edit me-1"></i>Adjust
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                        
                        <div class="mt-3">
                            <div class="row">
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <div class="h4 text-primary">@Model.PowderStocks.Sum(p => p.StockValue).ToString("C")</div>
                                        <small class="text-muted">Total Stock Value</small>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <div class="h4 text-info">@Model.PowderStocks.Sum(p => p.CurrentStock).ToString("F1") kg</div>
                                        <small class="text-muted">Total Stock Weight</small>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <div class="h4 text-success">@Model.PowderStocks.Count(p => !p.IsLowStock)</div>
                                        <small class="text-muted">Materials In Stock</small>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <div class="h4 text-warning">@Model.PowderStocks.Count(p => p.IsLowStock)</div>
                                        <small class="text-muted">Materials Low Stock</small>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-5">
                            <i class="fas fa-flask fa-3x text-muted mb-3"></i>
                            <h5 class="text-muted">No Powder Materials</h5>
                            <p class="text-muted">Start by adding your first powder material.</p>
                            <button type="button" class="btn btn-primary" onclick="showAddMaterialModal()">
                                <i class="fas fa-plus me-1"></i>Add Material
                            </button>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>

    <!-- Recent Usage History -->
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header bg-info text-white d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fas fa-history me-2"></i>
                        Recent Usage History
                    </h5>
                    <small>Last 30 days</small>
                </div>
                <div class="card-body">
                    @if (Model.RecentConsumption.Any())
                    {
                        <div class="table-responsive">
                            <table class="table table-sm">
                                <thead>
                                    <tr>
                                        <th>Date</th>
                                        <th>Part</th>
                                        <th>Material</th>
                                        <th class="text-center">Amount Used</th>
                                        <th class="text-center">Cost</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach (var consumption in Model.RecentConsumption.Take(15))
                                    {
                                        <tr>
                                            <td>
                                                <small>@consumption.UsageDate.ToString("MMM dd, HH:mm")</small>
                                            </td>
                                            <td>
                                                <div class="fw-bold">@consumption.PartNumber</div>
                                                <small class="text-muted">@consumption.PartName</small>
                                            </td>
                                            <td>@consumption.MaterialType</td>
                                            <td class="text-center">@consumption.AmountUsed.ToString("F1") kg</td>
                                            <td class="text-center">$@consumption.CostAllocated.ToString("F2")</td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                        
                        @if (Model.RecentConsumption.Count > 15)
                        {
                            <div class="text-center mt-3">
                                <button type="button" class="btn btn-outline-primary" onclick="showUsageReportModal()">
                                    <i class="fas fa-list me-1"></i>View All Usage History
                                </button>
                            </div>
                        }
                    }
                    else
                    {
                        <div class="text-center py-4">
                            <i class="fas fa-chart-bar fa-2x text-muted mb-3"></i>
                            <p class="text-muted mb-0">No usage history available yet.</p>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Add Material Modal -->
<div class="modal fade" id="addMaterialModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <form method="post" asp-page-handler="AddMaterial">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title">
                        <i class="fas fa-plus me-2"></i>Add New Material
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label for="MaterialType" class="form-label required">Material Type</label>
                        <input name="MaterialType" class="form-control" required 
                               placeholder="e.g., Ti-6Al-4V Grade 5" />
                    </div>
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="InitialStock" class="form-label">Initial Stock (kg)</label>
                            <input name="InitialStock" type="number" step="0.1" class="form-control" value="0" />
                        </div>
                        <div class="col-md-6">
                            <label for="ReorderPoint" class="form-label">Reorder Point (kg)</label>
                            <input name="ReorderPoint" type="number" step="0.1" class="form-control" value="10" />
                        </div>
                    </div>
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="CostPerKg" class="form-label">Cost per kg ($)</label>
                            <input name="CostPerKg" type="number" step="0.01" class="form-control" value="0" />
                        </div>
                        <div class="col-md-6">
                            <label for="AlertThreshold" class="form-label">Alert Threshold</label>
                            <select name="AlertThreshold" class="form-select">
                                <option value="0.25">25% of reorder point</option>
                                <option value="0.5">50% of reorder point</option>
                                <option value="1.0">At reorder point</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-success">
                        <i class="fas fa-plus me-1"></i>Add Material
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>

<!-- Restock Modal -->
<div class="modal fade" id="restockModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <form method="post" asp-page-handler="Restock">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">
                        <i class="fas fa-plus me-2"></i>Restock Material
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle me-2"></i>
                        Restocking <strong id="restockMaterialName"></strong>
                    </div>
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="AddedAmount" class="form-label required">Amount to Add (kg)</label>
                            <input name="AddedAmount" type="number" step="0.1" class="form-control" required min="0.1" />
                        </div>
                        <div class="col-md-6">
                            <label for="CostPerKg" class="form-label">Updated Cost per kg ($)</label>
                            <input name="CostPerKg" type="number" step="0.01" class="form-control" required min="0" />
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-primary">
                        <i class="fas fa-plus me-1"></i>Restock
                    </button>
                </div>
                <input type="hidden" name="StockId" id="restockStockId" />
            </form>
        </div>
    </div>
</div>

<!-- Update Stock Modal -->
<div class="modal fade" id="updateStockModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <form method="post" asp-page-handler="UpdateStock">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title">
                        <i class="fas fa-edit me-2"></i>Adjust Stock Level
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        Adjusting stock for <strong id="updateMaterialName"></strong>
                    </div>
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="NewAmount" class="form-label required">New Stock Amount (kg)</label>
                            <input name="NewAmount" type="number" step="0.1" class="form-control" required min="0" id="updateNewAmount" />
                        </div>
                        <div class="col-md-6">
                            <label for="Reason" class="form-label required">Reason for Adjustment</label>
                            <select name="Reason" class="form-select" required>
                                <option value="">Select reason</option>
                                <option value="Physical count correction">Physical count correction</option>
                                <option value="Damaged material removed">Damaged material removed</option>
                                <option value="Expired material removed">Expired material removed</option>
                                <option value="Found additional stock">Found additional stock</option>
                                <option value="Other">Other</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-warning">
                        <i class="fas fa-edit me-1"></i>Update Stock
                    </button>
                </div>
                <input type="hidden" name="StockId" id="updateStockId" />
            </form>
        </div>
    </div>
</div>

<!-- Usage Report Modal -->
<div class="modal fade" id="usageReportModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header bg-info text-white">
                <h5 class="modal-title">
                    <i class="fas fa-chart-bar me-2"></i>Powder Usage Report
                </h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form method="get" id="usageReportForm">
                    <div class="row mb-3">
                        <div class="col-md-4">
                            <label for="startDate" class="form-label">Start Date</label>
                            <input type="date" id="startDate" name="startDate" class="form-control" 
                                   value="@DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd")" />
                        </div>
                        <div class="col-md-4">
                            <label for="endDate" class="form-label">End Date</label>
                            <input type="date" id="endDate" name="endDate" class="form-control" 
                                   value="@DateTime.UtcNow.ToString("yyyy-MM-dd")" />
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">&nbsp;</label>
                            <button type="button" class="btn btn-primary d-block" onclick="generateUsageReport()">
                                <i class="fas fa-chart-bar me-1"></i>Generate Report
                            </button>
                        </div>
                    </div>
                </form>
                <div id="usageReportContent">
                    <!-- Report content will be loaded here -->
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
            </div>
        </div>
    </div>
</div>

<script>
function showAddMaterialModal() {
    $('#addMaterialModal').modal('show');
}

function showRestockModal(stockId, materialType) {
    $('#restockStockId').val(stockId);
    $('#restockMaterialName').text(materialType);
    $('#restockModal').modal('show');
}

function showUpdateStockModal(stockId, materialType, currentStock) {
    $('#updateStockId').val(stockId);
    $('#updateMaterialName').text(materialType);
    $('#updateNewAmount').val(currentStock);
    $('#updateStockModal').modal('show');
}

function showUsageReportModal() {
    $('#usageReportModal').modal('show');
    generateUsageReport(); // Load initial report
}

function generateUsageReport() {
    const startDate = $('#startDate').val();
    const endDate = $('#endDate').val();
    
    fetch(`/Admin/PowderInventory?handler=UsageReport&startDate=${startDate}&endDate=${endDate}`)
        .then(response => response.text())
        .then(html => {
            $('#usageReportContent').html(html);
        })
        .catch(error => {
            $('#usageReportContent').html('<div class="alert alert-danger">Error loading report</div>');
        });
}
</script>
```

#### 2B: Create Page Model
**File: `Pages/Admin/PowderInventory.cshtml.cs`**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services;
using OpCentrix.ViewModels.PowderInventory;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Roles = "Admin,Manager")]
    public class PowderInventoryModel : PageModel
    {
        private readonly PowderInventoryService _powderService;
        private readonly ILogger<PowderInventoryModel> _logger;

        public PowderInventoryModel(PowderInventoryService powderService, ILogger<PowderInventoryModel> logger)
        {
            _powderService = powderService;
            _logger = logger;
        }

        public List<PowderStockViewModel> PowderStocks { get; set; } = new();
        public List<PowderConsumptionViewModel> RecentConsumption { get; set; } = new();

        public async Task OnGetAsync()
        {
            PowderStocks = await _powderService.GetAllPowderStockAsync();
            RecentConsumption = await _powderService.GetConsumptionHistoryAsync(30);
        }

        public async Task<IActionResult> OnPostAddMaterialAsync(AddMaterialDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToPage();
            }

            var result = await _powderService.AddNewMaterialAsync(dto);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestockAsync(int stockId, decimal addedAmount, decimal costPerKg)
        {
            var result = await _powderService.RestockAsync(stockId, addedAmount, costPerKg);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStockAsync(int stockId, decimal newAmount, string reason)
        {
            var result = await _powderService.UpdateStockAsync(stockId, newAmount, reason);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetUsageReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = await _powderService.GetUsageReportAsync(startDate, endDate);
            return Partial("_UsageReportPartial", report);
        }
    }
}
```

### Step 3: Create Usage Report Partial

#### 3A: Create Usage Report Partial View
**File: `Pages/Admin/_UsageReportPartial.cshtml`**

```html
@model OpCentrix.ViewModels.PowderInventory.PowderUsageReportViewModel

@if (Model.TotalConsumptions > 0)
{
    <div class="row mb-3">
        <div class="col-md-3">
            <div class="text-center">
                <div class="h4 text-primary">@Model.TotalConsumptions</div>
                <small class="text-muted">Total Uses</small>
            </div>
        </div>
        <div class="col-md-3">
            <div class="text-center">
                <div class="h4 text-info">@Model.TotalAmountUsed.ToString("F1") kg</div>
                <small class="text-muted">Total Used</small>
            </div>
        </div>
        <div class="col-md-3">
            <div class="text-center">
                <div class="h4 text-success">@Model.TotalCost.ToString("C")</div>
                <small class="text-muted">Total Cost</small>
            </div>
        </div>
        <div class="col-md-3">
            <div class="text-center">
                <div class="h4 text-secondary">@Model.MaterialTotals.Count</div>
                <small class="text-muted">Materials Used</small>
            </div>
        </div>
    </div>

    <h6>Usage by Material</h6>
    <div class="table-responsive">
        <table class="table table-sm">
            <thead>
                <tr>
                    <th>Material Type</th>
                    <th class="text-center">Uses</th>
                    <th class="text-center">Total Used</th>
                    <th class="text-center">Average per Use</th>
                    <th class="text-center">Total Cost</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var material in Model.MaterialTotals)
                {
                    <tr>
                        <td class="fw-bold">@material.MaterialType</td>
                        <td class="text-center">@material.UsageCount</td>
                        <td class="text-center">@material.TotalUsed.ToString("F1") kg</td>
                        <td class="text-center">@((material.TotalUsed / material.UsageCount).ToString("F1")) kg</td>
                        <td class="text-center">@material.TotalCost.ToString("C")</td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
else
{
    <div class="text-center py-4">
        <i class="fas fa-chart-bar fa-2x text-muted mb-3"></i>
        <p class="text-muted">No powder usage in selected date range.</p>
    </div>
}
```

### Step 4: Add Powder Alerts to Navigation

#### 4A: Update Layout with Powder Alerts
**File: `Pages/Shared/_Layout.cshtml`**

Add powder stock alerts to the header:

```html
<!-- In the header, after existing notifications -->
<li class="nav-item dropdown">
    <a class="nav-link dropdown-toggle" href="#" id="powderAlertsDropdown" role="button" data-bs-toggle="dropdown">
        <i class="fas fa-flask"></i>
        <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-warning" id="lowStockBadge" style="display: none;">
            0
        </span>
    </a>
    <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="powderAlertsDropdown">
        <li><h6 class="dropdown-header">Powder Inventory</h6></li>
        <div id="lowStockAlerts">
            <!-- Low stock alerts will be loaded here -->
        </div>
        <li><hr class="dropdown-divider"></li>
        <li>
            <a class="dropdown-item" href="/Admin/PowderInventory">
                <i class="fas fa-cog me-2"></i>Manage Inventory
            </a>
        </li>
    </ul>
</li>

<script>
// Load low stock alerts periodically
function loadPowderAlerts() {
    fetch('/api/powder/low-stock-alerts')
        .then(response => response.json())
        .then(alerts => {
            const badge = document.getElementById('lowStockBadge');
            const alertsContainer = document.getElementById('lowStockAlerts');
            
            if (alerts.length > 0) {
                badge.textContent = alerts.length;
                badge.style.display = 'block';
                
                const alertsHtml = alerts.map(alert => `
                    <li>
                        <div class="dropdown-item-text">
                            <small class="text-warning">
                                <i class="fas fa-exclamation-triangle me-1"></i>
                                ${alert.materialType}: ${alert.currentStock} kg
                            </small>
                        </div>
                    </li>
                `).join('');
                
                alertsContainer.innerHTML = alertsHtml;
            } else {
                badge.style.display = 'none';
                alertsContainer.innerHTML = '<li><div class="dropdown-item-text"><small class="text-muted">All stock levels good</small></div></li>';
            }
        });
}

// Load alerts on page load and every 5 minutes
loadPowderAlerts();
setInterval(loadPowderAlerts, 300000); // 5 minutes
</script>
```

### Step 5: Create Powder Alerts API

#### 5A: Create Powder Alerts API Controller
**File: `Controllers/Api/PowderController.cs`**

```csharp
using Microsoft.AspNetCore.Mvc;
using OpCentrix.Services;

namespace OpCentrix.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PowderController : ControllerBase
    {
        private readonly PowderInventoryService _powderService;

        public PowderController(PowderInventoryService powderService)
        {
            _powderService = powderService;
        }

        [HttpGet("low-stock-alerts")]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            try
            {
                var lowStockAlerts = await _powderService.GetLowStockAlertsAsync();
                return Ok(lowStockAlerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve low stock alerts" });
            }
        }

        [HttpGet("stock-summary")]
        public async Task<IActionResult> GetStockSummary()
        {
            try
            {
                var stocks = await _powderService.GetAllPowderStockAsync();
                var summary = new
                {
                    totalValue = stocks.Sum(s => s.StockValue),
                    totalWeight = stocks.Sum(s => s.CurrentStock),
                    lowStockCount = stocks.Count(s => s.IsLowStock),
                    materials = stocks.Count
                };
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve stock summary" });
            }
        }
    }
}
```

### Step 6: Update Navigation Menu

#### 6A: Add Powder Inventory to Admin Menu
**File: `Pages/Shared/_Layout.cshtml`**

Add to the admin navigation menu:

```html
<!-- In admin dropdown menu -->
<li class="nav-item dropdown">
    <a class="nav-link dropdown-toggle" href="#" id="adminDropdown" role="button" data-bs-toggle="dropdown">
        <i class="fas fa-cogs"></i> Admin
    </a>
    <ul class="dropdown-menu" aria-labelledby="adminDropdown">
        <!-- ... existing menu items ... -->
        <li><hr class="dropdown-divider"></li>
        <li>
            <a class="dropdown-item" href="/Admin/PowderInventory">
                <i class="fas fa-flask me-2"></i>Powder Inventory
            </a>
        </li>
        <li>
            <a class="dropdown-item" href="/Admin/MachineAccuracy">
                <i class="fas fa-chart-line me-2"></i>Machine Accuracy
            </a>
        </li>
    </ul>
</li>
```

---

## Step 7: Testing & Validation

### 7A: Test Powder Stock Management
1. **Navigate to /Admin/PowderInventory**
2. **Add new material** - verify creation works
3. **Restock material** - test stock increase and cost update
4. **Adjust stock** - test manual stock adjustments
5. **Check low stock alerts** - verify alerts appear correctly

### 7B: Test Dashboard Integration
1. **View PrintTracking dashboard** - verify enhanced powder display
2. **Check navigation alerts** - low stock notifications work
3. **Test auto-refresh** - alerts update automatically
4. **Verify API endpoints** - low stock alerts API functional

### 7C: Test Usage Reporting
1. **Generate usage reports** - different date ranges
2. **Verify consumption tracking** - builds record powder usage
3. **Check cost allocation** - costs calculated correctly
4. **Test material analytics** - usage patterns shown

---

## Success Criteria

- ? **Comprehensive powder inventory management** - add, restock, adjust stock
- ? **Enhanced dashboard integration** - improved powder stock display
- ? **Low stock alerts functional** - navigation and dashboard alerts
- ? **Usage reporting complete** - detailed consumption analytics
- ? **API integration working** - real-time alerts and summaries
- ? **Admin interface complete** - full powder management capabilities
- ? **Cost tracking accurate** - proper cost allocation and reporting

## Next Steps

After completing this plan:
1. Test all powder management functionality thoroughly
2. Verify low stock alerts work correctly
3. Check usage reporting accuracy
4. Move to **Plan 9: Enhanced Stage-Specific Forms Validation**

---

**Status: READY FOR IMPLEMENTATION**  
**Next Plan: 09-Enhanced-Stage-Forms-Validation.md**