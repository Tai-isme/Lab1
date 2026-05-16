using Microsoft.AspNetCore.Mvc;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.API.Controllers;

/// <summary>
/// CRUD operations for enrollments. Supports expand for student and course.
/// </summary>
[ApiController]
[Route("api/enrollments")]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentController(IEnrollmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all enrollments with optional search, sort, paging, and expand.
    /// </summary>
    /// <response code="200">Returns the paginated list of enrollments.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var result = await _service.GetAllAsync(query);
        HttpContext.Items["Pagination"] = new
        {
            result.Page,
            result.PageSize,
            result.TotalItems,
            result.TotalPages
        };
        return Ok(result.Items);
    }

    /// <summary>
    /// Retrieves a single enrollment by its ID.
    /// </summary>
    /// <response code="200">Enrollment found and returned.</response>
    /// <response code="404">Enrollment not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var enrollment = await _service.GetByIdAsync(id, expand);
        if (enrollment == null)
            return NotFound();
        return Ok(enrollment);
    }

    /// <summary>
    /// Creates a new enrollment.
    /// </summary>
    /// <response code="201">Enrollment created successfully.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] EnrollmentRequest request)
    {
        var created = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing enrollment.
    /// </summary>
    /// <response code="200">Enrollment updated successfully.</response>
    /// <response code="404">Enrollment not found.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] EnrollmentRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Deletes an enrollment.
    /// </summary>
    /// <response code="200">Enrollment deleted successfully.</response>
    /// <response code="404">Enrollment not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return Ok(new { message = "Deleted successfully" });
    }
}
