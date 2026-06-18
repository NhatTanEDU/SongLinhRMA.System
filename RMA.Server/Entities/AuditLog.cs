using System;
using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class AuditLog
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string Action { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string User { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime Timestamp { get; set; }

        [FirestoreProperty]
        public string Details { get; set; } = string.Empty;

        [FirestoreProperty]
        public string OldValue { get; set; } = string.Empty;

        [FirestoreProperty]
        public string NewValue { get; set; } = string.Empty;
    }
}
