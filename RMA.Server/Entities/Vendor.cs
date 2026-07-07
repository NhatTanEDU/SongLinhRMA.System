using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class Vendor
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty; // Kết Nối Xanh, Nguyễn Kim, HP Service...

        [MaxLength(150)]
        [FirestoreProperty]
        public string? ContactPerson { get; set; } // Người liên hệ

        [MaxLength(50)]
        [FirestoreProperty]
        public string? Phone { get; set; } // SĐT hotline hoặc di động đối tác

        [MaxLength(150)]
        [FirestoreProperty]
        public string? Email { get; set; } // Thư điện tử hỗ trợ kỹ thuật

        [MaxLength(500)]
        [FirestoreProperty]
        public string? Address { get; set; } // Địa chỉ gửi hàng bảo hành

        [MaxLength(500)]
        [FirestoreProperty]
        public string? WarrantyLink { get; set; } // Link tra cứu S/N hãng nếu là TTBH ủy quyền

        [MaxLength(2000)]
        [FirestoreProperty]
        public string? Note { get; set; } // Ghi chú kinh nghiệm làm việc

        // Navigation properties
        public ICollection<RmaTicket> RmaTickets { get; set; } = new List<RmaTicket>();
    }
}
