﻿using Microsoft.EntityFrameworkCore;

namespace BlazorEcommerce.Server.Services.ProductService
{
	public class ProductService : IProductService
	{
		private readonly DataContext _context;
		public ProductService(DataContext context)
		{
			_context = context;
		}

		public async Task<ServiceResponse<List<string>>> GetProductSearchSuggestions(string searchText)
		{
			var products = await FindProductsBySearchText(searchText);
			List<string> result = new List<string>();

			foreach (var product in products)
			{
				if (product.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
				{
					result.Add(product.Title);
				}
				if (product.Description != null)
				{
					var punctuation = product.Description.Where(Char.IsPunctuation).Distinct().ToArray();
					var words = product.Description.Split().Select(s => s.Trim(punctuation));
					foreach (var word in words)
					{
						if (word.Contains(searchText, StringComparison.OrdinalIgnoreCase) && !result.Contains(word))
						{
							result.Add(word);
						}
					}
				}
			}

			return new ServiceResponse<List<string>>
			{
				Data = result
			};
		}

		public async Task<ServiceResponse<Product>> GetProductAsync(int productId)
		{
			var response = new ServiceResponse<Product>();
			var product = await _context.Products
				.Include(p => p.Variants.Where(v => v.Visible && !v.Deleted))
				.ThenInclude(v => v.ProductType)
				.FirstOrDefaultAsync(p => p.Id == productId && !p.Deleted && p.Visible);
			if (product == null)
			{
				response.Success = false;
				response.Message = "Sorry, but this product does not exist.";
			}
			else
			{
				response.Data = product;
			}
			return response;
		}

		public async Task<ServiceResponse<List<Product>>> GetProductsAsync()
		{
			var response = new ServiceResponse<List<Product>>()
			{
				Data = await _context.Products
				.Where(p => !p.Deleted && p.Visible)
				.Include(p => p.Variants.Where(p => !p.Deleted && p.Visible))
				.ToListAsync()
			};
			return response;
		}

		public async Task<ServiceResponse<List<Product>>> GetProductsByCategory(string categoryUrl)
		{
			var response = new ServiceResponse<List<Product>>
			{
				Data = await _context.Products
								.Where(p => p.Category.Url.ToLower().Equals(categoryUrl.ToLower()) && p.Visible && !p.Deleted)
								.Include(p => p.Variants.Where(v => !v.Deleted && v.Visible))
								.ToListAsync()
			};
			return response;
		}

		private Task<List<Product>> FindProductsBySearchText(string searchText)
		{
			return _context.Products
					.Where(p => p.Title.ToLower().Contains(searchText.ToLower()) ||
					p.Description.Contains(searchText.ToLower()) && p.Visible && !p.Deleted)
					.Include(p => p.Variants)
					.ToListAsync();
		}

		public async Task<ServiceResponse<ProductSearchResult>> SearchProducts(string searchText, int page)
		{
			var pageResults = 2f;
			var pageCount = Math.Ceiling((await FindProductsBySearchText(searchText)).Count / pageResults);
			var products = await _context.Products
					.Where(p => p.Title.ToLower().Contains(searchText.ToLower()) ||
					p.Description.Contains(searchText.ToLower()) && p.Visible && !p.Deleted)
					.Include(p => p.Variants)
					.Skip((page - 1) * (int)pageResults)
					.Take((int)pageResults)
					.ToListAsync();

			var response = new ServiceResponse<ProductSearchResult>
			{
				Data = new ProductSearchResult
				{
					Products = products,
					CurrentPage = page,
					Pages = (int)pageCount,
				}
			};
			return response;
		}

		public async Task<ServiceResponse<List<Product>>> GetFeaturedProducts()
		{
			var response = new ServiceResponse<List<Product>>
			{
				Data = await _context.Products
				.Where(p => p.Featured && p.Visible && !p.Deleted)
				.Include(p => p.Variants.Where(v => !v.Deleted && v.Visible))
				.ToListAsync()
			};
			return response;
		}
	}
}
