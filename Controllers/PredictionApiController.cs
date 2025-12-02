using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hulujan_Iulia_Petruta_lab4M.Models;
using Hulujan_Iulia_Petruta_lab4M.Data;

namespace Hulujan_Iulia_Petruta_lab4M.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PredictionApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PredictionApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PredictionHistory>>> GetAll()
        {
            var histories = await _context.PredictionHistories
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(histories);
        }

        // DELETE: api/PredictionApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var history = await _context.PredictionHistories.FindAsync(id);
            if (history == null)
            {
                return NotFound();
            }

            _context.PredictionHistories.Remove(history);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
