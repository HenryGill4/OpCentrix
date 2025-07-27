protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Make sure nullable string properties are properly configured
    modelBuilder.Entity<Part>(entity =>
    {
        // Configure properties that might be causing the NULL issue
        entity.Property(e => e.CreatedBy).HasDefaultValue("system");
        entity.Property(e => e.LastModifiedBy).IsRequired(false);
        entity.Property(e => e.AdminOverrideBy).IsRequired(false);
        entity.Property(e => e.AdminOverrideReason).IsRequired(false);
        // Add other nullable string properties as needed
    });
}