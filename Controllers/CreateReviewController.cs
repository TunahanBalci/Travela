using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelApp.Data;
using TravelApp.Models.Entities;
using TravelApp.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.Controllers
{
    public class CreateReviewController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CreateReviewController> _logger;

        public CreateReviewController(AppDBContext context, ILogger<CreateReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> CreateReview()
        {
            var model = await LoadDropdowns(new CreateReviewViewModel());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await LoadDropdowns(model);
                return View(model);
            }

            try
            {
                var review = new Review
                {
                    ID = Guid.NewGuid(),
                    Rating = model.Rating,
                    Comment = model.Comment,
                    UserID = model.UserID
                };

                switch (model.EntityType)
                {
                    case "Destination":
                        review.DestinationID = model.EntityID;
                        break;

                    case "Accommodation":
                        review.AccommodationID = model.EntityID;
                        break;

                    case "Activity":
                        review.ActivityID = model.EntityID;
                        break;

                    default:
                        throw new Exception("Invalid entity type.");
                }

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return RedirectToAction("ListReviews", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review.");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                model = await LoadDropdowns(model);
                return View(model);
            }
        }

        [HttpGet]
        [Route("CreateReview/EditReview/{id}")]
        public async Task<IActionResult> EditReview(Guid id)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Destination)
                    .Include(r => r.Accommodation)
                    .Include(r => r.Activity)
                    .FirstOrDefaultAsync(r => r.ID == id);

                if (review == null)
                {
                    return NotFound("Review not found.");
                }

                var viewModel = new CreateReviewViewModel
                {
                    Rating = review.Rating ?? 0,
                    Comment = review.Comment,
                    UserID = review.UserID,
                    EntityID = review.DestinationID ?? review.AccommodationID ?? review.ActivityID ?? Guid.Empty,
                    EntityType = review.DestinationID.HasValue ? "Destination" :
                                 review.AccommodationID.HasValue ? "Accommodation" :
                                 "Activity",
                    Users = await _context.Users
                        .Select(u => new SelectListItem
                        {
                            Value = u.ID.ToString(),
                            Text = $"{u.Name} (ID: {u.ID})"
                        })
                        .ToListAsync(),
                    Destinations = await _context.Destinations
                        .Select(d => new SelectListItem
                        {
                            Value = d.ID.ToString(),
                            Text = $"{d.Name} (ID: {d.ID})"
                        })
                        .ToListAsync(),
                    Accommodations = await _context.Accommodations
                        .Select(a => new SelectListItem
                        {
                            Value = a.ID.ToString(),
                            Text = $"{a.Name} (ID: {a.ID})"
                        })
                        .ToListAsync(),
                    Activities = await _context.Activities
                        .Select(a => new SelectListItem
                        {
                            Value = a.ID.ToString(),
                            Text = $"{a.Name} (ID: {a.ID})"
                        })
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review for editing.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(Guid id, CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await LoadDropdowns(model);
                return View(model);
            }

            try
            {
                var review = await _context.Reviews.FindAsync(id);

                if (review == null)
                {
                    return NotFound("Review not found.");
                }

                review.Rating = model.Rating;
                review.Comment = model.Comment;
                review.UserID = model.UserID;

                review.DestinationID = model.EntityType == "Destination" ? model.EntityID : null;
                review.AccommodationID = model.EntityType == "Accommodation" ? model.EntityID : null;
                review.ActivityID = model.EntityType == "Activity" ? model.EntityID : null;

                await _context.SaveChangesAsync();

                return RedirectToAction("ListReviews", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing review.");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                model = await LoadDropdowns(model);
                return View(model);
            }
        }

        private async Task<CreateReviewViewModel> LoadDropdowns(CreateReviewViewModel model)
        {
            model.Users = await _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.ID.ToString(),
                    Text = $"{u.Name} (ID: {u.ID})"
                })
                .ToListAsync();

            model.Destinations = await _context.Destinations
                .Select(d => new SelectListItem
                {
                    Value = d.ID.ToString(),
                    Text = $"{d.Name} (ID: {d.ID})"
                })
                .ToListAsync();

            model.Accommodations = await _context.Accommodations
                .Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = $"{a.Name} (ID: {a.ID})"
                })
                .ToListAsync();

            model.Activities = await _context.Activities
                .Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = $"{a.Name} (ID: {a.ID})"
                })
                .ToListAsync();

            return model;
        }

        [HttpGet]
        public async Task<IActionResult> GetEntities(string entityType)
        {
            try
            {
                if (string.IsNullOrEmpty(entityType))
                {
                    _logger.LogWarning("Entity type is empty.");
                    return BadRequest("Entity type is required.");
                }

                List<SelectListItem> entities;

                switch (entityType)
                {
                    case "Destination":
                        entities = await _context.Destinations
                            .Select(d => new SelectListItem
                            {
                                Value = d.ID.ToString(),
                                Text = d.Name
                            })
                            .ToListAsync();
                        break;

                    case "Accommodation":
                        entities = await _context.Accommodations
                            .Select(a => new SelectListItem
                            {
                                Value = a.ID.ToString(),
                                Text = a.Name
                            })
                            .ToListAsync();
                        break;

                    case "Activity":
                        entities = await _context.Activities
                            .Select(a => new SelectListItem
                            {
                                Value = a.ID.ToString(),
                                Text = a.Name
                            })
                            .ToListAsync();
                        break;

                    default:
                        _logger.LogWarning("Invalid entity type: {EntityType}", entityType);
                        return BadRequest("Invalid entity type.");
                }

                _logger.LogInformation("Successfully fetched entities for type: {EntityType}", entityType);
                return Json(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entities for type: {EntityType}", entityType);
                return StatusCode(500, "An error occurred while fetching entities.");
            }
        }

    }
}
