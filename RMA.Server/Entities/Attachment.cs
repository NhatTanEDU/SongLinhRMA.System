using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class Attachment
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string RmaTicketId { get; set; } = string.Empty;

        [ForeignKey(nameof(RmaTicketId))]
        public RmaTicket RmaTicket { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        [FirestoreProperty]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [FirestoreProperty]
        public string FileType { get; set; } = string.Empty; // SN_PHOTO, CONDITION_PHOTO

        [FirestoreProperty]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
