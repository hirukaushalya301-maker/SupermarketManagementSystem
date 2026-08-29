namespace SupermarketManagementSystem.Models
{
    public class InventoryDashboardSummary
    {
        public int TotalProducts { get; set; }

        public int LowStockProducts { get; set; }

        public int PendingPurchaseOrders { get; set; }

        public int ActiveSuppliers { get; set; }

        public int UnreadNotifications { get; set; }

        public int ExpiringBatches { get; set; }
    }
}