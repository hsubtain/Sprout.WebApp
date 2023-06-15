using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sprout.Exam.Business.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sprout.Exam.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeSecondController : ControllerBase
    {
        private List<EmployeeDto> employees = new List<EmployeeDto>()
        {
            new EmployeeDto() { Id = 0, FullName = "Harris Subtain", Birthdate = "05-22-1997", Tin = "2312342344", TypeId = 1},
            new EmployeeDto() { Id = 0, FullName = "Raabia Subtain", Birthdate = "10-20-1995", Tin = "6548789953", TypeId = 2}
        };

        [HttpGet]
        public IEnumerable<EmployeeDto> Get()
        {
            return employees;
        }
    }
}
