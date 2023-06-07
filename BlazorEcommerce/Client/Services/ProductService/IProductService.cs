
namespace BlazorEcommerce.Client.Services.ProductService
{
	public interface IProductService
	{
		event Action ProductsChanged;
		public List<Product> Products { get; set; }
		string Message { get; set; }
		public Task GetProducts(string? categoryUrl = null);
		public Task<ServiceResponse<Product>> GetProduct(int productId);

		public Task SearchProducts(string searchText);
		public Task<List<string>> GetProductSearchSuggestions(string searchText);

	}
}
