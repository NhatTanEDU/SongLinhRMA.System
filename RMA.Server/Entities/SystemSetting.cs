using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class SystemSetting
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string Key { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string Value { get; set; } = string.Empty;
    }
}
