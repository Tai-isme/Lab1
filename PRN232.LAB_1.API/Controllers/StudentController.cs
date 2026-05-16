using Microsoft.AspNetCore.Mvc;
using PRN232.LAB_1.API.Models;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.API.Controllers;

/// <summary>
/// CRUD operations for students.
/// </summary>
[ApiController]
[Route("api/students")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentController(IStudentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all students with optional search, sort, and paging.
    /// </summary>
    /// <response code="200">Returns the paginated list of students.</response>
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
    /// Retrieves a single student by its ID.
    /// </summary>
    /// <response code="200">Student found and returned.</response>
    /// <response code="404">Student not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var student = await _service.GetByIdAsync(id, expand);
        if (student == null)
            return NotFound();
        return Ok(student);
    }

    /// <summary>
    /// Creates a new student.
    /// </summary>
    /// <response code="201">Student created successfully.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] StudentRequest request)
    {
        var created = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing student.
    /// </summary>
    /// <response code="200">Student updated successfully.</response>
    /// <response code="404">Student not found.</response>
    /// <response code="400">Invalid input.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a student.
    /// </summary>
    /// <response code="200">Student deleted successfully.</response>
    /// <response code="404">Student not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return Ok(ApiResponse<object>.Ok(new { message = "Deleted successfully" }));
    }
}
