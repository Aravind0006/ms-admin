using ms_admin.Dbconnection;

namespace ms_admin.Services
{
    public interface IProductService
    {
        // Define your service methods here
        Task<bool> UpdateProductStatusAsync(int productId, string status);
    }

    public class ProductService : IProductService
    {
        private readonly userdbconnection _context;

        public ProductService(userdbconnection context)
        {
            _context = context;
        }

        // Implement your service methods here
        public async Task<bool> UpdateProductStatusAsync(int productId, string status)
        {
            try
            {
                // Find the product by productId
                var product = await _context.ProductDtos.FindAsync(productId);

                if (product == null)
                {
                    // Product not found
                    return false;
                }

                // Update the product status
                product.Status = status;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Update successful
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return false;
            }
        }
    }
}
