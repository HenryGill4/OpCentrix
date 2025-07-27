-- Database Fix Script for NULL Values in Parts Table
-- This script updates any NULL values in string fields to empty strings
-- Run this script if you encounter "The data is NULL at ordinal 22" errors

UPDATE Parts 
SET 
    AdminOverrideReason = COALESCE(AdminOverrideReason, ''),
    AdminOverrideBy = COALESCE(AdminOverrideBy, ''),
    Description = COALESCE(Description, ''),
    CustomerPartNumber = COALESCE(CustomerPartNumber, ''),
    Industry = COALESCE(Industry, 'General'),
    Application = COALESCE(Application, 'General Component'),
    PartClass = COALESCE(PartClass, 'B'),
    CreatedBy = COALESCE(CreatedBy, 'System'),
    LastModifiedBy = COALESCE(LastModifiedBy, 'System'),
    AvgDuration = COALESCE(AvgDuration, '8h 0m'),
    Dimensions = COALESCE(Dimensions, ''),
    PowderSpecification = COALESCE(PowderSpecification, '15-45 micron particle size'),
    SurfaceFinishRequirement = COALESCE(SurfaceFinishRequirement, 'As-built'),
    ProcessType = COALESCE(ProcessType, 'SLS Metal'),
    RequiredMachineType = COALESCE(RequiredMachineType, 'TruPrint 3000'),
    PreferredMachines = COALESCE(PreferredMachines, 'TI1,TI2'),
    QualityStandards = COALESCE(QualityStandards, 'ASTM F3001, ISO 17296'),
    ToleranceRequirements = COALESCE(ToleranceRequirements, '±0.1mm typical, ±0.05mm critical dimensions'),
    RequiredSkills = COALESCE(RequiredSkills, 'SLS Operation,Powder Handling,Inert Gas Safety,Post-Processing'),
    RequiredCertifications = COALESCE(RequiredCertifications, 'SLS Operation Certification,Powder Safety Training'),
    RequiredTooling = COALESCE(RequiredTooling, 'Build Platform,Powder Sieve,Support Removal Tools'),
    ConsumableMaterials = COALESCE(ConsumableMaterials, 'Argon Gas,Build Platform Coating'),
    SupportStrategy = COALESCE(SupportStrategy, 'Minimal supports on overhangs > 45°'),
    ProcessParameters = COALESCE(ProcessParameters, '{}'),
    QualityCheckpoints = COALESCE(QualityCheckpoints, '{}'),
    BuildFileTemplate = COALESCE(BuildFileTemplate, ''),
    CadFilePath = COALESCE(CadFilePath, ''),
    CadFileVersion = COALESCE(CadFileVersion, '')
WHERE 
    AdminOverrideReason IS NULL 
    OR AdminOverrideBy IS NULL
    OR Description IS NULL
    OR CustomerPartNumber IS NULL
    OR Industry IS NULL
    OR Application IS NULL
    OR PartClass IS NULL
    OR CreatedBy IS NULL
    OR LastModifiedBy IS NULL
    OR AvgDuration IS NULL
    OR Dimensions IS NULL
    OR PowderSpecification IS NULL
    OR SurfaceFinishRequirement IS NULL
    OR ProcessType IS NULL
    OR RequiredMachineType IS NULL
    OR PreferredMachines IS NULL
    OR QualityStandards IS NULL
    OR ToleranceRequirements IS NULL
    OR RequiredSkills IS NULL
    OR RequiredCertifications IS NULL
    OR RequiredTooling IS NULL
    OR ConsumableMaterials IS NULL
    OR SupportStrategy IS NULL
    OR ProcessParameters IS NULL
    OR QualityCheckpoints IS NULL
    OR BuildFileTemplate IS NULL
    OR CadFilePath IS NULL
    OR CadFileVersion IS NULL;