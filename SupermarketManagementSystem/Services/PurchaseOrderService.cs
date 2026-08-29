using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class PurchaseOrderService
    {
        private readonly PurchaseOrderRepository repository;

        public PurchaseOrderService()
        {
            repository = new PurchaseOrderRepository();
        }

        public List<PurchaseOrder> GetAllOrders()
        {
            return repository.GetAllOrders();
        }

        public List<PurchaseOrderItem> GetOrderItems(
            int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
            {
                return new List<PurchaseOrderItem>();
            }

            return repository.GetOrderItems(
                purchaseOrderId
            );
        }

        public string GenerateOrderNumber()
        {
            return "PO-" +
                   DateTime.Now.ToString(
                       "yyyyMMdd-HHmmss"
                   );
        }

        public OperationResult CreateOrder(
            PurchaseOrder order)
        {
            PrepareOrder(order);

            OperationResult validation =
                ValidateOrder(order);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.OrderNumberExists(
                order.OrderNumber))
            {
                return Failure(
                    "This purchase order number already exists."
                );
            }

            try
            {
                order.OrderStatus = "DRAFT";

                order.PurchaseOrderId =
                    repository.CreateOrder(order);

                return order.PurchaseOrderId > 0
                    ? Success(
                        "Purchase order created successfully."
                    )
                    : Failure(
                        "The purchase order could not be created."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to create purchase order: " +
                    ex.Message
                );
            }
        }

        public OperationResult UpdateDraftOrder(
            PurchaseOrder order)
        {
            if (order.PurchaseOrderId <= 0)
            {
                return Failure(
                    "Please select a purchase order."
                );
            }

            PrepareOrder(order);

            OperationResult validation =
                ValidateOrder(order);

            if (!validation.IsSuccessful)
            {
                return validation;
            }

            if (repository.OrderNumberExists(
                order.OrderNumber,
                order.PurchaseOrderId))
            {
                return Failure(
                    "This purchase order number already exists."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateDraftOrder(order);

                return updated
                    ? Success(
                        "Purchase order updated successfully."
                    )
                    : Failure(
                        "Only a DRAFT purchase order " +
                        "can be edited."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update purchase order: " +
                    ex.Message
                );
            }
        }

        public OperationResult ChangeOrderStatus(
            int purchaseOrderId,
            string status)
        {
            if (purchaseOrderId <= 0)
            {
                return Failure(
                    "Please select a purchase order."
                );
            }

            status = status.Trim().ToUpperInvariant();

            string[] validStatuses =
            {
                "DRAFT",
                "SENT",
                "CONFIRMED",
                "DECLINED",
                "PROCESSING",
                "PARTIALLY_DELIVERED",
                "DELIVERED",
                "CANCELLED"
            };

            if (!validStatuses.Contains(status))
            {
                return Failure(
                    "Invalid purchase order status."
                );
            }

            try
            {
                bool updated =
                    repository.UpdateOrderStatus(
                        purchaseOrderId,
                        status
                    );

                return updated
                    ? Success(
                        "Purchase order status updated successfully."
                    )
                    : Failure(
                        "The purchase order was not found."
                    );
            }
            catch (Exception ex)
            {
                return Failure(
                    "Unable to update order status: " +
                    ex.Message
                );
            }
        }

        private static void PrepareOrder(
            PurchaseOrder order)
        {
            order.OrderNumber =
                order.OrderNumber
                    .Trim()
                    .ToUpperInvariant();

            order.SupplierResponseNote =
                order.SupplierResponseNote.Trim();

            foreach (PurchaseOrderItem item in order.Items)
            {
                item.ReceivedQuantity =
                    Math.Max(0, item.ReceivedQuantity);
            }

            order.Subtotal =
                order.Items.Sum(item =>
                    item.LineTotal
                );

            order.TotalAmount =
                order.Subtotal +
                order.TaxAmount;
        }

        private static OperationResult ValidateOrder(
            PurchaseOrder order)
        {
            if (string.IsNullOrWhiteSpace(
                order.OrderNumber))
            {
                return Failure(
                    "Purchase order number is required."
                );
            }

            if (order.OrderNumber.Length > 40)
            {
                return Failure(
                    "Order number cannot exceed 40 characters."
                );
            }

            if (order.SupplierId <= 0)
            {
                return Failure(
                    "Please select a supplier."
                );
            }

            if (order.ExpectedDeliveryDate.HasValue &&
                order.ExpectedDeliveryDate.Value.Date <
                order.OrderDate.Date)
            {
                return Failure(
                    "Expected delivery date cannot be " +
                    "before the order date."
                );
            }

            if (order.Items.Count == 0)
            {
                return Failure(
                    "Add at least one product to the order."
                );
            }

            if (order.Items
                .GroupBy(item => item.ProductId)
                .Any(group => group.Count() > 1))
            {
                return Failure(
                    "The same product cannot be added twice."
                );
            }

            foreach (PurchaseOrderItem item in order.Items)
            {
                if (item.ProductId <= 0)
                {
                    return Failure(
                        "An order item has an invalid product."
                    );
                }

                if (item.OrderedQuantity <= 0)
                {
                    return Failure(
                        "Ordered quantity must be greater than zero."
                    );
                }

                if (item.UnitCost < 0)
                {
                    return Failure(
                        "Unit cost cannot be negative."
                    );
                }

                if (item.ReceivedQuantity >
                    item.OrderedQuantity)
                {
                    return Failure(
                        "Received quantity cannot exceed " +
                        "ordered quantity."
                    );
                }
            }

            if (order.TaxAmount < 0)
            {
                return Failure(
                    "Tax amount cannot be negative."
                );
            }

            return Success("Valid");
        }

        private static OperationResult Success(
            string message)
        {
            return new OperationResult
            {
                IsSuccessful = true,
                Message = message
            };
        }

        private static OperationResult Failure(
            string message)
        {
            return new OperationResult
            {
                IsSuccessful = false,
                Message = message
            };
        }
    }
}