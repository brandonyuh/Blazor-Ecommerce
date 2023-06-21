using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorEcommerce.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CategoryController : ControllerBase
	{
		private readonly ICategoryService _categoryService;

		public CategoryController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		[HttpGet]
		public async Task<ActionResult<ServiceResponse<List<Category>>>> GetCategories()
		{
			return Ok(await _categoryService.GetCategories());
		}

		[HttpGet("admin"), Authorize(Roles="Admin")]
		public async Task<ActionResult<ServiceResponse<List<Category>>>> GetAdminCategories()
		{
			return Ok(await _categoryService.GetAdminCategories());
		}

		[HttpDelete("admin/{id}"), Authorize(Roles = "Admin")]
		public async Task<ActionResult<ServiceResponse<List<Category>>>> DeleteCategory(int id)
		{
			return Ok(await _categoryService.DeleteCategory(id));
		}

		[HttpPost("admin"), Authorize(Roles = "Admin")]
		public async Task<ActionResult<ServiceResponse<List<Category>>>> AddCategory(Category category)
		{
			return Ok(await _categoryService.AddCategory(category));
		}
		[HttpPut("admin"), Authorize(Roles = "Admin")]
		public async Task<ActionResult<ServiceResponse<List<Category>>>> UpdateCategory(Category category)
		{
			return Ok(await _categoryService.UpdateCategory(category));
		}

	}
}
