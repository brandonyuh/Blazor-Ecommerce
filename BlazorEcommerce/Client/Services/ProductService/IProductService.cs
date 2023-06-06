
namespace BlazorEcommerce.Client.Services.ProductService
{
	public interface IProductService
	{
		event Action ProductsChanged;
		public List<Product> Products { get; set; }
		public Task GetProducts(string? categoryUrl = null);
		public Task<ServiceResponse<Product>> GetProduct(int productId);
	}
}
