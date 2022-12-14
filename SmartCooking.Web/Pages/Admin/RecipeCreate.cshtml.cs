using Microsoft.AspNetCore.Mvc;
using SmartCooking.Common.Enumeration;
using SmartCooking.Common.Extensions;
using SmartCooking.Data.Repository;
using SmartCooking.Infastructure.Products;
using SmartCooking.Infastructure.Recipes;
using SmartCooking.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartCooking.Web.Pages.Admin
{
	public class RecipeCreateModel : AdminPageModel
	{
		private readonly IRecipeRepository recipeRepository;
		private readonly IItemRepository itemRepository;
		private readonly IUnitRepository unitRepository;
		private readonly IRecipeImageRepository recipeImageRepository;

		[BindProperty] public int DraftRecipeHeaderId { get; set; }
		[BindProperty] public RecipeHeader RecipeHeader { get; set; }
		[BindProperty] public IEnumerable<RecipeDetail> RecipeDetails { get; set; }
		[BindProperty] public IEnumerable<Unit> UnitsList { get; set; }
		[BindProperty] public IEnumerable<Item> ItemsList { get; set; }
		[BindProperty] public IEnumerable<RecipeImage> RecipeImages { get; set; }

		public RecipeCreateModel(IRecipeRepository recipeRepository, IItemRepository itemRepository, IUnitRepository unitRepository, IRecipeImageRepository recipeImageRepository)
		{
			this.recipeRepository = recipeRepository;
			this.itemRepository = itemRepository;
			this.unitRepository = unitRepository;
			this.recipeImageRepository = recipeImageRepository;
		}

		public async Task<IActionResult> OnGetAsync(int? draftHeaderId)
		{
			if (!CheckPermissions())
			{
				return RedirectToPage(Url.Content("~/Home/Index"));
			}

			if (draftHeaderId.HasValue)
			{
				RecipeHeader = await recipeRepository.GetRecipeHeader(draftHeaderId.Value);
				RecipeDetails = await recipeRepository.GetRecipeDetails(RecipeHeader.Id);
				DraftRecipeHeaderId = draftHeaderId.Value;
			}

			if (RecipeHeader is null)
			{
				RecipeHeader = new RecipeHeader
				{
					RecipeType = SmartCooking.Common.Enumeration.RecipeType.Draft
				};
				await recipeRepository.InsertRecipeHeader(RecipeHeader);

				return RedirectToPage("RecipeCreate", new { draftHeaderId = RecipeHeader.Id });
			}

			RecipeImages = await recipeImageRepository.GetRecipeImages(draftHeaderId.Value);

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			RecipeHeader.RecipeType = Common.Enumeration.RecipeType.Done;

			if (await recipeRepository.UpdateRecipeHeader(RecipeHeader))
			{
				foreach (RecipeDetail recipeDetail in RecipeDetails)
				{
					if (!await recipeRepository.UpdateRecipeDetail(recipeDetail))
					{
						HasError = true;
						ViewData["Error"] = "???? ???? ?????? ??? ??? ?????? ?? ??????????? ? ???????.";
						return Page();
					}
				}

				TempData["SuccessMessage"] = "? ??????? ?????????? ?? ????????.";
				return RedirectToPage(Url.Content("~/Admin/RecipeList"));
			}

			HasError = true;
			ViewData["Error"] = "???? ???? ?????? ??? ??? ?????? ?? ??????????? ? ???????.";
			return Page();
		}

		public async Task<IActionResult> OnGetAddToList(string itemName, string unitName, string qty, string recipeId)
		{
			try
			{
				Item itemObj = new Item();
				Unit unitObj = new Unit();

				IEnumerable<Item> itemList = await itemRepository.GetItems();
				IEnumerable<Unit> unitList = await unitRepository.GetUnits();

				if (!itemList.Any(x => x.Name.ToLower().StartsWith(itemName.ToLower().Trim())))
				{
					itemObj.Name = itemName.Trim();
					await itemRepository.InsertItem(itemObj);
				}
				else
				{
					itemObj = itemList.FirstOrDefault(x => x.Name.ToLower().StartsWith(itemName.ToLower().Trim()));
				}

				if (!unitList.Any(x => x.Name.ToLower().StartsWith(unitName.ToLower().Trim())))
				{
					unitObj.Name = unitName.Trim();
					await unitRepository.InsertUnit(unitObj);
				}
				else
				{
					unitObj = unitList.FirstOrDefault(x => x.Name.ToLower().StartsWith(unitName.ToLower().Trim()));
				}

				RecipeDetail recipeDetail = new RecipeDetail()
				{
					ItemId = itemObj.Id,
					UnitId = unitObj.Id,
					Quantity = float.Parse(qty),
					RecipeHeaderId = recipeId.ToInt()
				};

				await recipeRepository.InsertRecipeDetail(recipeDetail);

				return new JsonResult(new { result = true, entityId = recipeDetail.Id });
			}
			catch (Exception ex)
			{
				return new JsonResult(new { result = false, message = ex.Message });
			}
		}
	}
}