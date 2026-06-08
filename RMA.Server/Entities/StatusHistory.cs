using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class StatusHistory
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string RmaTicketId { get; set; } = string.Empty;

        [ForeignKey(nameof(RmaTicketId))]
        public RmaTicket RmaTicket { get; set; } = null!;

        [FirestoreProperty]
        public string? LocationId { get; set; }

        [FirestoreProperty]
        public string? StatusId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        [FirestoreProperty]
        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        [FirestoreProperty]
        public string? Note { get; set; }
    }
}
