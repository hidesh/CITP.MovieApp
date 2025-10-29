using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Authorize] // Requires login for all note actions
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly INoteRepository _notes;

        public NotesController(INoteRepository notes)
        {
            _notes = notes;
        }

        private int GetUserIdOrThrow()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(id))
                throw new UnauthorizedAccessException();
            return int.Parse(id);
        }

        // Get all notes for the logged-in user
        [HttpGet("user")]
        public async Task<IActionResult> GetMyNotes()
        {
            var userId = GetUserIdOrThrow();
            var notes = await _notes.GetAllForUserAsync(userId);
            return Ok(notes);
        }

        // Get all notes for a movie by the logged-in user
        [HttpGet("movie/{tconst}")]
        public async Task<IActionResult> GetMovieNotes(string tconst)
        {
            var userId = GetUserIdOrThrow();
            var notes = await _notes.GetAllForUserByMovieAsync(userId, tconst);
            return Ok(notes);
        }

        // Get all notes for a person by the logged-in user
        [HttpGet("person/{nconst}")]
        public async Task<IActionResult> GetPersonNotes(string nconst)
        {
            var userId = GetUserIdOrThrow();
            var notes = await _notes.GetAllForUserByPersonAsync(userId, nconst);
            return Ok(notes);
        }

        // Create note for a movie
        [HttpPost("movie/{tconst}")]
        public async Task<IActionResult> CreateForMovie(string tconst, [FromBody] NoteCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Content is required." });

            var userId = GetUserIdOrThrow();
            var id = await _notes.CreateForMovieAsync(userId, tconst, dto);
            return CreatedAtAction(nameof(GetMovieNotes), new { tconst }, new { id });
        }

        // Create note for a person
        [HttpPost("person/{nconst}")]
        public async Task<IActionResult> CreateForPerson(string nconst, [FromBody] NoteCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Content is required." });

            var userId = GetUserIdOrThrow();
            var id = await _notes.CreateForPersonAsync(userId, nconst, dto);
            return CreatedAtAction(nameof(GetPersonNotes), new { nconst }, new { id });
        }

        // Update a note
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] NoteUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Content is required." });

            var userId = GetUserIdOrThrow();
            var ok = await _notes.UpdateAsync(id, userId, dto);
            return ok ? NoContent() : NotFound();
        }

        // Delete a note
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserIdOrThrow();
            var ok = await _notes.DeleteAsync(id, userId);
            return ok ? NoContent() : NotFound();
        }
    }
}
