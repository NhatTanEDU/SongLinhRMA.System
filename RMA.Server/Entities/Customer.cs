using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class Customer
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        [FirestoreProperty]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        [FirestoreProperty]
        public string? Phone { get; set; }

        [MaxLength(255)]
        [FirestoreProperty]
        public string? Email { get; set; }

        [MaxLength(500)]
        [FirestoreProperty]
        public string? Address { get; set; }

        [MaxLength(500)]
        [FirestoreProperty]
        public string? AvatarUrl { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
