using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Controller-level authorization requires a valid login for all endpoints
    public class DataController : ControllerBase
    {
        // Accessible only by users with the "Admin" role.
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAdminData()
        {
            return Ok(new { Message = "This is top-secret data for Admins only! 🤫" });
        }

        // Accessible by users with either "Admin" or "Employee" roles.
        [HttpGet("staff")]
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult GetStaffData()
        {
            return Ok(new { Message = "This is internal data for Admins and Employees. 🧑‍💼" });
        }

        // Accessible by any logged-in user, including "Customer".
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new { Message = "This data is available to all logged-in users. ✅" });
        }
    }
}