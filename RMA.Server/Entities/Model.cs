using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class Model
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string CategoryId { get; set; } = string.Empty;

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        [MaxLength(100)]
        [FirestoreProperty]
        public string? Brand { get; set; }

        [Required]
        [MaxLength(255)]
        [FirestoreProperty]
        public string ModelName { get; set; } = string.Empty;

        [FirestoreProperty]
        public int StockQuantity { get; set; }

        [FirestoreProperty]
        public int WarrantyMonths { get; set; }

        [FirestoreProperty]
        public bool IsSerialRequired { get; set; }

        // Navigation properties
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
