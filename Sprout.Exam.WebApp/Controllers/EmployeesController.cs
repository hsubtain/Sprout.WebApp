using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Sprout.Exam.Business.DataTransferObjects;
using Sprout.Exam.Common.Enums;
using Sprout.Exam.WebApp.Models;
using Sprout.Exam.WebApp.Data;

namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public EmployeesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<EmployeeDto> Get()
        {
            var result = _dbContext.Employees;
            return result;
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public EmployeeDto GetById(int id)
        {
            var employeeByIdResult = _dbContext.Employees.FirstOrDefault(employee => employee.Id == id);
            return employeeByIdResult;
        }

        /// <summary>
        /// Refactor this method to go through proper layers and update changes to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(EditEmployeeDto input)
        {
            var findResult =_dbContext.Employees.Find(input.Id);
            findResult.FullName = input.FullName;
            findResult.Birthdate = input.Birthdate;
            findResult.Tin = input.Tin;
            findResult.TypeId = input.TypeId;
            _dbContext.SaveChanges();
            return Ok(findResult);
        }

        //[HttpPut("{id}")]
        //public IActionResult PutSecond([FromBody] employee)
        //{
        //    var retMessage = "Successfully Created Employee";
        //    /PERFORM UPDATE/
        //    return Ok(retMessage);
        //}


        /// <summary>
        /// Refactor this method to go through proper layers and insert employees to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(EmployeeDto input)
        {

            _dbContext.Employees.Add(input);
            _dbContext.SaveChanges();
            int lastID = _dbContext.Employees.Max(employee => employee.Id);

            return Created($"/api/employees/{lastID}", lastID);
        }

        //[HttpPost]
        //public IActionResult PostSecond(EmployeeDto employee)
        //{
        //    var retMessage = "Successfully Created Employee";
        //    employees.Add(employee);
        //    return Ok(retMessage);
        //}


        /// <summary>
        /// Refactor this method to go through proper layers and perform soft deletion of an employee to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await Task.FromResult(StaticEmployees.ResultList.FirstOrDefault(m => m.Id == id));
            if (result == null) return NotFound();
            StaticEmployees.ResultList.RemoveAll(m => m.Id == id);
            return Ok(id);
        }

        //[HttpDelete("{id}")]
        //public IActionResult DeleteSecond(int id)
        //{
        //    var retMessage = "Successfully Created Employee";
        //    /PERFORM DELETE/
        //    employees.RemoveAt(id);
        //    return Ok(retMessage);
        //}


        /// <summary>
        /// Refactor this method to go through proper layers and use Factory pattern
        /// </summary>
        /// <param name="id"></param>
        /// <param name="absentDays"></param>
        /// <param name="workedDays"></param>
        /// <returns></returns>
        [HttpPost("{id}/calculate")]
        public async Task<IActionResult> Calculate(int id,decimal absentDays,decimal workedDays)
        {
            var result = await Task.FromResult(StaticEmployees.ResultList.FirstOrDefault(m => m.Id == id));

            if (result == null) return NotFound();
            var type = (EmployeeType) result.TypeId;
            return type switch
            {
                EmployeeType.Regular =>
                    //create computation for regular.
                    Ok(25000),
                EmployeeType.Contractual =>
                    //create computation for contractual.
                    Ok(20000),
                _ => NotFound("Employee Type not found")
            };

        }

    }
}
