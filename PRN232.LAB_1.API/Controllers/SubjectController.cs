using Microsoft.AspNetCore.Mvc;
using PRN232.LAB_1.API.Models;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.API.Controllers;

/// <summary>
/// CRUD operations for academic subjects.
/// </summary>
[ApiController]
[Route("api/subjects")]
public class SubjectController : ControllerBase
{
    private readonly ISubjectService _service;

    public SubjectController(ISubjectService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all subjects with optional search, sort, and paging.
    /// </summary>
    /// <response code="200">Returns the paginated list of subjects.</response>
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
    /// Retrieves a single subject by its ID.
    /// </summary>
    /// <response code="200">Subject found and returned.</response>
    /// <response code="404">Subject not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var subject = await _service.GetByIdAsync(id, expand);
        if (subject == null)
            return NotFound();
        return Ok(subject);
    }

    /// <summary>
    /// Creates a new subject.
    /// </summary>
    /// <response code="201">Subject created successfully.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SubjectRequest request)
    {
        var created = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing subject.
    /// </summary>
    /// <response code="200">Subject updated successfully.</response>
    /// <response code="404">Subject not found.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] SubjectRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a subject.
    /// </summary>
    /// <response code="200">Subject deleted successfully.</response>
    /// <response code="404">Subject not found.</response>
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
