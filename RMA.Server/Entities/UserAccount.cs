using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class UserAccount
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string Username { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [FirestoreProperty]
        public string Role { get; set; } = string.Empty; // Admin, Sales, Tech, Manager

        [FirestoreProperty]
        public string Status { get; set; } = "Active"; // Active, Inactive

        [FirestoreProperty]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
