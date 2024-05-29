using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Racnarock_explorer.Models;
using Racnarock_explorer.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Racnarock_explorer.Pages
{
    public class AddTourModel : PageModel
    {
        private readonly ILogger<AddTourModel> _logger;
        private readonly FileUploadService _fileUploadService;
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "tours.json");

        public AddTourModel(ILogger<AddTourModel> logger, FileUploadService fileUploadService)
        {
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        [BindProperty]
        public Tour Tour { get; set; }

        [BindProperty]
        public IFormFile AudioFile { get; set; }

        public IActionResult OnGet(int? id)
        {
            if (HttpContext.Session.GetString("LoggedInUser") != "admin")
            {
                return RedirectToPage("/Login");
            }

            if (id.HasValue)
            {
                var tour = GetTourById(id.Value);
                if (tour == null)
                {
                    return NotFound();
                }

                Tour = tour;
            }
            else
            {
                Tour = new Tour();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (id.HasValue)
            {
                var existingTour = GetTourById(id.Value);
                if (existingTour == null)
                {
                    return NotFound();
                }

                if (AudioFile != null)
                {
                    var filePath = await _fileUploadService.UploadFileAsync(AudioFile, Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Music"));
                    if (filePath == null)
                    {
                        ModelState.AddModelError("", "Error uploading file.");
                        return Page();
                    }

                    existingTour.AudioUrl = filePath;
                }

                existingTour.Title = Tour.Title;
                existingTour.Description = Tour.Description;

                UpdateTour(existingTour);
                TempData["SuccessMessage"] = "Tour updated successfully!";
            }
            else
            {
                if (AudioFile == null)
                {
                    ModelState.AddModelError("AudioFile", "Please select a file to upload.");
                    return Page();
                }

                var filePath = await _fileUploadService.UploadFileAsync(AudioFile, Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Music"));
                if (filePath == null)
                {
                    ModelState.AddModelError("", "Error uploading file.");
                    return Page();
                }

                Tour.AudioUrl = filePath;
                SaveTour(Tour);
                TempData["SuccessMessage"] = "Tour added successfully!";
            }

            return RedirectToPage("/Tours");
        }

        private Tour GetTourById(int id)
        {
            if (System.IO.File.Exists(_filePath))
            {
                var json = System.IO.File.ReadAllText(_filePath);
                var tours = JsonSerializer.Deserialize<List<Tour>>(json);
                return tours?.FirstOrDefault(t => t.Id == id);
            }
            return null;
        }

        private void SaveTour(Tour tour)
        {
            var tours = new List<Tour>();
            if (System.IO.File.Exists(_filePath))
            {
                var json = System.IO.File.ReadAllText(_filePath);
                tours = JsonSerializer.Deserialize<List<Tour>>(json);
            }

            tour.Id = tours.Count > 0 ? tours[^1].Id + 1 : 1;
            tours.Add(tour);

            var updatedJson = JsonSerializer.Serialize(tours, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_filePath, updatedJson);
        }

        private void UpdateTour(Tour updatedTour)
        {
            var json = System.IO.File.ReadAllText(_filePath);
            var tours = JsonSerializer.Deserialize<List<Tour>>(json);

            var tourIndex = tours.FindIndex(t => t.Id == updatedTour.Id);
            if (tourIndex >= 0)
            {
                tours[tourIndex] = updatedTour;
            }

            var updatedJson = JsonSerializer.Serialize(tours, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_filePath, updatedJson);
        }
    }
}
