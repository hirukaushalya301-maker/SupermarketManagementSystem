using SupermarketManagementSystem.Models;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class InventoryDashboardService
    {
        private readonly InventoryDashboardRepository
            repository;

        public InventoryDashboardService()
        {
            repository =
                new InventoryDashboardRepository();
        }

        public InventoryDashboardSummary GetSummary()
        {
            try
            {
                return repository.GetSummary();
            }
            catch (Exception)
            {
                // Keeps the dashboard usable if its summary
                // information cannot be loaded.
                return new InventoryDashboardSummary();
            }
        }
    }
}