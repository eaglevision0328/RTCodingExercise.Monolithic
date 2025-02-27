using Microsoft.AspNetCore.Mvc;
using RTCodingExercise.Monolithic.Interfaces;
using RTCodingExercise.Monolithic.Models;
using System.Diagnostics;
using System.Text.Json;

namespace RTCodingExercise.Monolithic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPlateService _plateService;

        public HomeController(ILogger<HomeController> logger, IPlateService plateService)
        {
            _logger = logger;
            _plateService = plateService;
        }

        public async Task<IActionResult> Index(
            string sortOrder,
            string letterFilter,
            string numberFilter,
            string promoCode,
            int pageNumber = 1)
        {
            int pageSize = 10;

            var promoDetails = _plateService.GetPromoDetails(promoCode);
            ViewData["PromoCode"] = promoCode;
            ViewData["IsPercentDiscount"] = promoDetails.IsPercentDiscount;
            ViewData["Discount"] = promoDetails.Discount;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["LetterFilter"] = letterFilter;
            ViewData["NumberFilter"] = numberFilter;

            ViewData["PriceSortParam"] = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewData["SalePriceSortParam"] = sortOrder == "sale_price" ? "sale_price_desc" : "sale_price";

            ViewData["PriceSort"] = sortOrder == "price_desc" ? "▼" :
                                   string.IsNullOrEmpty(sortOrder) ? "▲" : "";
            ViewData["SalePriceSort"] = sortOrder == "sale_price_desc" ? "▼" :
                                        sortOrder == "sale_price" ? "▲" : "";

            var plates = await _plateService.GetFilteredAndSortedPlates(sortOrder, letterFilter, numberFilter);

            return View(PaginatedList<Plate>.Create(plates, pageNumber, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> AddPlate(Plate plate)
        {
            if (ModelState.IsValid)
            {
                await _plateService.AddPlateAsync(plate);

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus([FromBody] JsonElement data)
        {
            try
            {
                string plateIdString = data.GetProperty("plateId").GetString();
                Guid plateId = Guid.Parse(plateIdString);
                string status = data.GetProperty("status").GetString();

                await _plateService.UpdateReservationStatusAsync(plateId, status);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plate status");
                return BadRequest(ex.Message);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}