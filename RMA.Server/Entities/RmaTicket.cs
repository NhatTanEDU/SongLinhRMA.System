using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class RmaTicket
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string StatusId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? VendorId { get; set; }

        [Required]
        [MaxLength(2000)]
        [FirestoreProperty]
        public string ProblemDescription { get; set; } = string.Empty;

        [MaxLength(100)]
        [FirestoreProperty]
        public string? ServiceMode { get; set; } // Warranty hoặc Repair

        [FirestoreProperty]
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? SentDate { get; set; } // Ngày gửi đi - Mốc 14 ngày

        [FirestoreProperty]
        public bool IsUrgent { get; set; } = false;

        [MaxLength(2000)]
        [FirestoreProperty]
        public string? StaffNote { get; set; }

        [MaxLength(500)]
        [FirestoreProperty]
        public string? EndUserName { get; set; }
    }
}
