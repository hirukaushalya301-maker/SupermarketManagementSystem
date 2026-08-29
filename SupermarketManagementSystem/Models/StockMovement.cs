namespace SupermarketManagementSystem.Models
{
    public class StockMovement
    {
        public long MovementId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } =
            string.Empty;

        public int? BatchId { get; set; }

        public string BatchNumber { get; set; } =
            string.Empty;

        public string MovementType { get; set; } =
            string.Empty;

        public int Quantity { get; set; }

        public string ReferenceType { get; set; } =
            string.Empty;

        public long? ReferenceId { get; set; }

        public string Notes { get; set; } =
            string.Empty;

        public int? PerformedBy { get; set; }

        public string PerformedByName { get; set; } =
            string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsStockIncrease
        {
            get
            {
                return MovementType == "PURCHASE" ||
                       MovementType == "RETURN_IN" ||
                       MovementType == "ADJUSTMENT_IN";
            }
        }

        public bool IsStockDecrease
        {
            get
            {
                return MovementType == "SALE" ||
                       MovementType == "RETURN_OUT" ||
                       MovementType == "ADJUSTMENT_OUT" ||
                       MovementType == "EXPIRED";
            }
        }
    }
}