using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class Device
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [FirestoreProperty]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string ModelId { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime? PurchaseDate { get; set; }

        [FirestoreProperty]
        public DateTime? WarrantyExpiry { get; set; }

        [FirestoreProperty]
        public string? OrderId { get; set; }

        [FirestoreProperty]
        public string? OrderCode { get; set; }
    }
}
