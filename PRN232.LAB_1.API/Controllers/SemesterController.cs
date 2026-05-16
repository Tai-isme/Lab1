using Microsoft.AspNetCore.Mvc;
using PRN232.LAB_1.API.Models;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.API.Controllers;

/// <summary>
/// CRUD operations for academic semesters.
/// </summary>
[ApiController]
[Route("api/semesters")]
public class SemesterController : ControllerBase
{
    private readonly ISemesterService _service;

    public SemesterController(ISemesterService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all semesters with optional search, sort, and paging.
    /// </summary>
    /// <param name="query">Query parameters: search, sort, page, size, fields, expand.</param>
    /// <returns>A paginated list of semesters wrapped in the response envelope.</returns>
    /// <response code="200">Returns the paginated list of semesters.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    /// Retrieves a single semester by its ID.
    /// </summary>
    /// <param name="id">The semester ID.</param>
    /// <param name="expand">Optional comma-separated list of related entities to include.</param>
    /// <returns>The semester if found; otherwise 404.</returns>
    /// <response code="200">Semester found and returned.</response>
    /// <response code="404">Semester not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var semester = await _service.GetByIdAsync(id, expand);
        if (semester == null)
            return NotFound();
        return Ok(semester);
    }

    /// <summary>
    /// Creates a new semester.
    /// </summary>
    /// <param name="request">Semester details.</param>
    /// <returns>The created semester with 201 status.</returns>
    /// <response code="201">Semester created successfully.</response>
    /// <response code="400">Invalid input — validation errors in envelope.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SemesterRequest request)
    {
        var created = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing semester.
    /// </summary>
    /// <param name="id">The semester ID.</param>
    /// <param name="request">Updated semester details.</param>
    /// <returns>The updated semester if found; otherwise 404.</returns>
    /// <response code="200">Semester updated successfully.</response>
    /// <response code="404">Semester not found.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] SemesterRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a semester.
    /// </summary>
    /// <param name="id">The semester ID.</param>
    /// <returns>Success message if deleted; otherwise 404.</returns>
    /// <response code="200">Semester deleted successfully.</response>
    /// <response code="404">Semester not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return Ok(ApiResponse<object>.Ok(new { message = "Deleted successfully" }));
    }
}
