namespace RMA.Shared.DTOs
{
    public class StatusMasterDto
    {
        public string Id { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
    }

    public class VendorDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class CategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class ModelDto
    {
        public string Id { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int WarrantyMonths { get; set; }
        public bool IsSerialRequired { get; set; }
    }

    public class LocationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
    }
}
