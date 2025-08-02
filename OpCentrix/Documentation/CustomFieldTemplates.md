# ?? **OpCentrix Custom Field Templates Documentation**

## ?? **Overview**

The OpCentrix Manufacturing Execution System includes comprehensive custom field templates for all major production stages. These templates provide standardized, industry-specific field configurations that ensure consistent data collection across different manufacturing processes.

---

## ?? **Available Stage Templates**

### **1. 3D Printing Template**
**Purpose**: Additive manufacturing using SLS, FDM, DMLS, and other 3D printing technologies

**Key Custom Fields**:
- **Print Technology**: SLS Metal, SLS Polymer, FDM, DMLS, EBM, SLA, DLP
- **Material Selection**: Ti-6Al-4V Grade 5, 316L Stainless Steel, Inconel 718, etc.
- **Process Parameters**: Layer Height (10-100µm), Laser Power (50-100%), Scan Speed (800-2000mm/s)
- **Build Settings**: Build Temperature (150-200°C), Support Structures (Yes/No)
- **Post-Processing**: Support Removal, Powder Removal, Heat Treatment

**Usage Example**:
```json
{
  "printTechnology": "SLS Metal",
  "material": "Ti-6Al-4V Grade 5",
  "layerHeight": "30",
  "laserPower": "75",
  "scanSpeed": "1200",
  "buildTemperature": "180",
  "supportStructures": true,
  "postProcessing": "Support Removal"
}
```

### **2. CNC Machining Template**
**Purpose**: Computer numerical control machining for precision operations

**Key Custom Fields**:
- **Machine Type**: 3-Axis Mill, 4-Axis Mill, 5-Axis Mill, CNC Lathe, Swiss Lathe
- **Operations**: Facing, Drilling, Tapping, Boring, Contouring, Pocketing, Threading
- **Tolerances**: ±0.005", ±0.002", ±0.001", ±0.0005", ±0.0002"
- **Surface Finish**: 125 RMS, 63 RMS, 32 RMS, 16 RMS, 8 RMS, 4 RMS
- **Workholding**: Machine Vise, Chuck, Collet, Custom Fixture
- **Coolant Type**: Flood Coolant, Mist Coolant, Air Blast, MQL

**Usage Example**:
```json
{
  "machineType": "3-Axis Mill",
  "operations": "Facing",
  "tolerances": "±0.002\"",
  "surfaceFinish": "63 RMS",
  "workholding": "Machine Vise",
  "coolant": "Flood Coolant"
}
```

### **3. EDM Operations Template**
**Purpose**: Electrical discharge machining for complex geometries and hard materials

**Key Custom Fields**:
- **EDM Type**: Wire EDM, Sinker EDM, Hole Drilling EDM, Fast Hole EDM
- **Electrode Type**: Copper, Graphite, Copper Tungsten, Silver Tungsten, Brass
- **Wire Specifications**: Brass Wire, Coated Wire, Zinc Coated (0.004"-0.012" diameter)
- **Dielectric Fluid**: Deionized Water, Oil-based, Hydrocarbon
- **Surface Finish**: Rough (125+ RMS), Semi-Finish (32-63 RMS), Finish (16-32 RMS)
- **Tolerances**: ±0.0005", ±0.0002", ±0.0001", ±0.00005"

### **4. Laser Engraving Template**
**Purpose**: Laser engraving for serial numbers, markings, and regulatory compliance

**Key Custom Fields**:
- **Laser Type**: Fiber Laser, CO2 Laser, UV Laser, Green Laser, Diode Laser
- **Marking Type**: Serial Number, Part Number, Logo, Date Code, QR Code, Barcode
- **Engraving Depth**: 0.001"-0.010" (customizable)
- **Font Size**: 0.050"-0.500" (for text markings)
- **Marking Location**: Descriptive text field
- **Regulatory Compliance**: ATF/ITAR compliant marking checkbox

### **5. Sandblasting Template**
**Purpose**: Abrasive blasting for surface preparation and finish uniformity

**Key Custom Fields**:
- **Blast Media**: Aluminum Oxide, Glass Beads, Steel Shot, Ceramic Beads, Walnut Shells
- **Media Mesh Size**: 16-30, 24-46, 36-60, 46-100, 80-120, 120-220 Mesh
- **Blast Pressure**: 40-120 PSI (customizable)
- **Surface Profile**: Light (1-2 mils), Medium (2-4 mils), Heavy (4-6 mils), Very Heavy (6+ mils)
- **Masking Required**: Yes/No checkbox
- **Post-Blast Cleaning**: Compressed Air, Solvent Wipe, Ultrasonic Cleaning

### **6. Coating Template**
**Purpose**: Surface coating application including Cerakote, anodizing, and protective finishes

**Key Custom Fields**:
- **Coating Type**: Cerakote, Anodizing, Powder Coating, Electroplating, PVD, DLC, Parkerizing
- **Coating Color**: Matte Black, Gloss Black, FDE, OD Green, Tungsten, Burnt Bronze
- **Coating Thickness**: 0.0002"-0.002" (customizable)
- **Surface Preparation**: Sandblast, Chemical Clean, Solvent Degrease, Acid Etch
- **Cure Temperature**: 200-400°F (customizable)
- **Cure Time**: 60-240 minutes (customizable)
- **Quality Standard**: MIL-DTL-5541, ASTM B117, ASTM D3359, ISO 12944

### **7. Assembly Template**
**Purpose**: Final assembly with components, hardware, and sub-assemblies

**Key Custom Fields**:
- **Assembly Type**: Mechanical, Threaded, Press Fit, Welded, Bonded, Hybrid
- **Component Count**: 2-50 components (customizable)
- **Torque Specifications**: Text field for detailed torque values
- **Special Tools Required**: Text field for tool requirements
- **Thread Sealant Required**: Yes/No checkbox
- **Functional Testing**: None, Basic Function Check, Pressure Test, Leak Test, Performance Test
- **Calibration Required**: Yes/No checkbox
- **Packaging Requirements**: Standard Box, Anti-Static Bag, Foam Protection, Custom Case

### **8. Shipping Template**
**Purpose**: Final packaging and shipping preparation with documentation

**Key Custom Fields**:
- **Shipping Method**: Ground, 2-Day Air, Next Day Air, International, Freight
- **Packaging Type**: Standard Box, Padded Envelope, Custom Crate, Anti-Static Package
- **Insurance Value**: $0-$50,000 (customizable)
- **Special Handling Required**: Yes/No checkbox
- **Required Documentation**: Certificate of Compliance, Material Cert, Test Report, ATF Form
- **Tracking Required**: Yes/No checkbox
- **Signature Required**: Yes/No checkbox
- **Export Controlled Item**: Yes/No checkbox for ITAR/EAR compliance

---

## ?? **Implementation Guide**

### **1. Service Registration**
The `StageTemplateService` is automatically registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IStageTemplateService, StageTemplateService>();
```

### **2. Using Templates in Code**

#### **Get All Available Templates**:
```csharp
public async Task<IActionResult> OnGetAsync()
{
    var templates = await _stageTemplateService.GetAllStageTemplatesAsync();
    // Process templates...
}
```

#### **Create a Stage from Template**:
```csharp
public async Task<ProductionStage> CreateStageAsync(string templateName)
{
    var stage = await _stageTemplateService.CreateStageFromTemplateAsync(templateName);
    
    // Save to database
    _context.ProductionStages.Add(stage);
    await _context.SaveChangesAsync();
    
    return stage;
}
```

#### **Apply Template Custom Fields to a Part**:
```csharp
public async Task ApplyTemplateToPartAsync(int partId, string stageName)
{
    var customFields = await _stageTemplateService.GetCustomFieldsForStageAsync(stageName);
    
    // Create PartStageRequirement with custom field values
    var partStage = new PartStageRequirement
    {
        PartId = partId,
        ProductionStageId = stageId,
        // Set custom field values based on template
    };
    
    // Apply default values from template
    var customFieldValues = new Dictionary<string, object>();
    foreach (var field in customFields)
    {
        customFieldValues[field.Name] = field.DefaultValue ?? GetDefaultValueForType(field.Type);
    }
    
    partStage.SetCustomFieldValues(customFieldValues);
}
```

### **3. Frontend Integration**

#### **Display Custom Fields in Forms**:
```javascript
async function loadStageCustomFields(stageName) {
    const response = await fetch(`/admin/productionstages/templates?handler=TemplateCustomFields&stageName=${stageName}`);
    const data = await response.json();
    
    if (data.success) {
        renderCustomFieldsForm(data.customFields);
    }
}

function renderCustomFieldsForm(customFields) {
    const container = document.getElementById('customFieldsContainer');
    
    customFields.forEach(field => {
        const fieldHtml = generateFieldHtml(field);
        container.appendChild(fieldHtml);
    });
}

function generateFieldHtml(field) {
    switch (field.type) {
        case 'dropdown':
            return createDropdownField(field);
        case 'number':
            return createNumberField(field);
        case 'checkbox':
            return createCheckboxField(field);
        case 'textarea':
            return createTextareaField(field);
        default:
            return createTextField(field);
    }
}
```

---

## ?? **API Endpoints**

### **Get All Templates**
```
GET /admin/productionstages/templates?handler=TemplatePreview
```

**Response**:
```json
{
  "success": true,
  "totalTemplates": 8,
  "totalCustomFields": 67,
  "templates": [
    {
      "name": "3D Printing",
      "displayOrder": 1,
      "description": "Additive manufacturing using SLS, FDM, or DMLS technologies",
      "stageColor": "#007bff",
      "stageIcon": "fas fa-print",
      "customFields": [...]
    }
  ]
}
```

### **Get Template Custom Fields**
```
GET /admin/productionstages/templates?handler=TemplateCustomFields&stageName=3D%20Printing
```

**Response**:
```json
{
  "success": true,
  "stageName": "3D Printing",
  "customFields": [
    {
      "name": "printTechnology",
      "type": "dropdown",
      "label": "Print Technology",
      "required": true,
      "options": ["SLS Metal", "SLS Polymer", "FDM", "DMLS"],
      "defaultValue": "SLS Metal"
    }
  ]
}
```

### **Apply Template to Part**
```
POST /admin/productionstages/templates?handler=ApplyTemplateToPart
Content-Type: application/x-www-form-urlencoded

partId=123&stageName=3D%20Printing
```

---

## ?? **Customization Options**

### **Adding New Templates**
To add a new stage template, modify the `GetStageTemplates()` method in `StageTemplateService.cs`:

```csharp
new ProductionStageTemplate
{
    Name = "Your New Stage",
    DisplayOrder = 9,
    Description = "Description of your new stage",
    DefaultSetupMinutes = 30,
    DefaultHourlyRate = 85.00m,
    DefaultDurationHours = 2.0,
    StageColor = "#28a745",
    StageIcon = "fas fa-your-icon",
    Department = "Your Department",
    CustomFields = new List<CustomFieldDefinition>
    {
        new CustomFieldDefinition
        {
            Name = "yourField",
            Type = "text",
            Label = "Your Field Label",
            Description = "Field description",
            Required = true,
            DefaultValue = "Default value",
            DisplayOrder = 1
        }
    }
}
```

### **Custom Field Types Supported**
- **text**: Single-line text input
- **textarea**: Multi-line text input
- **number**: Numeric input with min/max validation
- **dropdown**: Select from predefined options
- **checkbox**: Boolean true/false selection
- **date**: Date picker (can be added)

### **Field Validation Options**
- **Required**: Mark field as mandatory
- **MinValue/MaxValue**: For numeric fields
- **Options**: For dropdown fields
- **ValidationPattern**: Regex pattern for text fields
- **Unit**: Display unit for measurements

---

## ?? **Testing the Templates**

### **1. Access the Templates Page**
Navigate to: `/admin/productionstages/templates`

### **2. View Template Details**
Click "View Details" on any template card to see all custom fields with their specifications.

### **3. Test Template Creation**
Click "Create Stage" to demonstrate creating a production stage from a template.

### **4. API Testing**
Use the browser developer tools to test the API endpoints:

```javascript
// Test in browser console
fetch('/admin/productionstages/templates?handler=TemplatePreview')
  .then(response => response.json())
  .then(data => console.log('Templates:', data));
```

---

## ?? **Benefits of Using Templates**

### **? Standardization**
- Consistent field definitions across all manufacturing stages
- Standardized data collection for analytics and reporting
- Reduced training time for operators

### **? Industry Best Practices**
- Fields based on real manufacturing requirements
- Compliance with quality standards (MIL-STD, ASTM, ISO)
- Regulatory compliance support (ATF/ITAR)

### **? Flexibility**
- Easy customization of field values per part
- Template-based creation reduces setup time
- Extensible architecture for adding new templates

### **? Quality Assurance**
- Required field validation ensures complete data
- Dropdown options prevent data entry errors
- Unit specifications reduce measurement confusion

---

## ?? **Future Enhancements**

### **Planned Features**
- **Template Versioning**: Track changes to templates over time
- **Custom Template Editor**: GUI for creating custom templates
- **Template Import/Export**: Share templates between systems
- **Field Dependencies**: Show/hide fields based on other field values
- **Calculated Fields**: Auto-calculate values based on other inputs
- **Template Analytics**: Track usage statistics for optimization

### **Integration Opportunities**
- **ERP Integration**: Sync with enterprise resource planning systems
- **MES Integration**: Connect with manufacturing execution systems
- **Quality Management**: Link with quality management systems
- **Regulatory Compliance**: Automated compliance reporting

---

## ?? **Best Practices**

### **Template Design**
1. **Keep fields relevant**: Only include fields that add value
2. **Use clear labels**: Make field purposes obvious to operators
3. **Provide good defaults**: Reduce data entry burden
4. **Group related fields**: Organize fields logically
5. **Include help text**: Provide descriptions for complex fields

### **Implementation**
1. **Test thoroughly**: Validate all field types and validations
2. **Train operators**: Ensure users understand field purposes
3. **Monitor usage**: Track which fields are actually used
4. **Iterate based on feedback**: Continuously improve templates
5. **Document changes**: Keep records of template modifications

---

**?? Documentation Version**: 1.0  
**?? Last Updated**: January 30, 2025  
**?? Created By**: GitHub Copilot Assistant  
**?? System**: OpCentrix Manufacturing Execution System