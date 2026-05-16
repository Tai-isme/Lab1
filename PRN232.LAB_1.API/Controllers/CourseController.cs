using Microsoft.AspNetCore.Mvc;
using PRN232.LAB_1.API.Models;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.API.Controllers;

/// <summary>
/// CRUD operations for courses. Supports expand for subject and semester.
/// </summary>
[ApiController]
[Route("api/courses")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _service;

    public CourseController(ICourseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all courses with optional search, sort, paging, and expand.
    /// </summary>
    /// <response code="200">Returns the paginated list of courses.</response>
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
    /// Retrieves a single course by its ID.
    /// </summary>
    /// <response code="200">Course found and returned.</response>
    /// <response code="404">Course not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var course = await _service.GetByIdAsync(id, expand);
        if (course == null)
            return NotFound();
        return Ok(course);
    }

    /// <summary>
    /// Creates a new course.
    /// </summary>
    /// <response code="201">Course created successfully.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CourseRequest request)
    {
        var created = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing course.
    /// </summary>
    /// <response code="200">Course updated successfully.</response>
    /// <response code="404">Course not found.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] CourseRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a course.
    /// </summary>
    /// <response code="200">Course deleted successfully.</response>
    /// <response code="404">Course not found.</response>
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
