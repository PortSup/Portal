using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorPortalAPI.Data;
using VendorPortalAPI.Models;

namespace VendorPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorProfileController : ControllerBase
    {
        private readonly VendorDbContext _context;

        public VendorProfileController(VendorDbContext context)
        {
            _context = context;
        }

        // GET: api/VendorProfile
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorProfile>>> GetVendorProfiles()
        {
            return await _context.VendorProfiles.ToListAsync();
        }

        // GET: api/VendorProfile/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VendorProfile>> GetVendorProfile(int id)
        {
            var vendorProfile = await _context.VendorProfiles.FindAsync(id);

            if (vendorProfile == null)
            {
                return NotFound();
            }

            return vendorProfile;
        }

        // POST: api/VendorProfile
        [HttpPost]
        public async Task<ActionResult<VendorProfile>> PostVendorProfile(VendorProfile vendorProfile)
        {
            _context.VendorProfiles.Add(vendorProfile);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVendorProfile", new { id = vendorProfile.Id }, vendorProfile);
        }

        // PUT: api/VendorProfile/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVendorProfile(int id, VendorProfile vendorProfile)
        {
            if (id != vendorProfile.Id)
            {
                return BadRequest();
            }

            _context.Entry(vendorProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorProfileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool VendorProfileExists(int id)
        {
            return _context.VendorProfiles.Any(e => e.Id == id);
        }
    }
}