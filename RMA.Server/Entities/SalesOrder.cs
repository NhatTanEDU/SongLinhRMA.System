using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace RMA.Server.Entities
{
    [FirestoreData]
    public class SalesOrder
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string OrderCode { get; set; } = string.Empty;

        [FirestoreProperty]
        public string CustomerId { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime OrderDate { get; set; }

        [FirestoreProperty]
        public DateTime? DeliveryDate { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } = "Pending";

        [FirestoreProperty]
        public string SalesNote { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Note { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime? LastUpdated { get; set; }

        [FirestoreProperty]
        public string UpdatedBy { get; set; } = string.Empty;

        [FirestoreProperty]
        public List<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    }

    [FirestoreData]
    public class OrderDetail
    {
        [FirestoreProperty]
        public string ModelId { get; set; } = string.Empty;

        [FirestoreProperty]
        public int Quantity { get; set; }

        [FirestoreProperty]
        public int WarrantyMonths { get; set; }

        [FirestoreProperty]
        public string DeviceSpecs { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Note { get; set; } = string.Empty;
    }
}
