using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class Brand
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty; // Dell, HP, ASUS...
    }
}
