﻿using Microsoft.AspNetCore.Mvc;
using TravelApp.Data;
using TravelApp.Models.Entities;
using TravelApp.Models.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TravelApp.Controllers
{
    [Authorize]
    public class CreateAmenityController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CreateAmenityController> _logger;

        public CreateAmenityController(AppDBContext context, ILogger<CreateAmenityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> CreateAmenity()
        {
            try
            {
                var model = new CreateAmenityViewModel
                {
                    Accommodations = await _context.Accommodations.ToListAsync()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Create Amenity page.");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAmenity(CreateAmenityViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var amenity = new Amenity
                    {
                        ID = Guid.NewGuid(),
                        Name = model.Name
                    };

                    _context.Amenities.Add(amenity);

                    if (model.SelectedAccommodationIds != null && model.SelectedAccommodationIds.Any())
                    {
                        var selectedAccommodations = await _context.Accommodations
                            .Where(a => model.SelectedAccommodationIds.Contains(a.ID))
                            .ToListAsync();

                        foreach (var accommodation in selectedAccommodations)
                        {
                            accommodation.Amenities.Add(amenity);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("ListAmenities", "AdminPanel");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Amenity.");
                    ModelState.AddModelError("", "An error occurred while creating the Amenity. Please try again.");
                }
            }

            model.Accommodations = await _context.Accommodations.ToListAsync();
            return View(model);
        }

        [HttpGet]
        [Route("CreateAmenity/EditAmenity/{id}")]
        public async Task<IActionResult> EditAmenity(Guid id)
        {
            try
            {
                var amenity = await _context.Amenities
                    .Include(a => a.Accommodations)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (amenity == null)
                {
                    return NotFound("Amenity not found.");
                }

                var viewModel = new CreateAmenityViewModel
                {
                    Name = amenity.Name,
                    SelectedAccommodationIds = amenity.Accommodations.Select(a => a.ID).ToList(),
                    Accommodations = await _context.Accommodations.ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Edit Amenity page for ID {ID}.", id);
                return StatusCode(500, "An error occurred while loading the page.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAmenity(Guid id, CreateAmenityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Accommodations = await _context.Accommodations.ToListAsync();
                return View(model);
            }

            try
            {
                var amenity = await _context.Amenities
                    .Include(a => a.Accommodations)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (amenity == null)
                {
                    ModelState.AddModelError("", "Amenity not found.");
                    return View(model);
                }

                amenity.Name = model.Name;

                var selectedAccommodations = await _context.Accommodations
                    .Where(a => model.SelectedAccommodationIds.Contains(a.ID))
                    .ToListAsync();

                amenity.Accommodations.Clear();
                foreach (var accommodation in selectedAccommodations)
                {
                    amenity.Accommodations.Add(accommodation);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("ListAmenities", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Amenity with ID {ID}.", id);
                ModelState.AddModelError("", "An error occurred while updating the amenity. Please try again.");
                model.Accommodations = await _context.Accommodations.ToListAsync();
                return View(model);
            }
        }
    }
}
