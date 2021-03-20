using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeePayrollWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // GET: api/Test    
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var empList = new List<string>();
                empList = null;
                var countItem = empList.Count();
                return Ok(empList);
            }
            catch (Exception ex)
            {
                return StatusCode(HttpContext.Response.StatusCode, "Internal server error");
            }
        }
    }
}
