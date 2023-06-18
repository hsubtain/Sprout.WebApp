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
using System.Net.Http;

namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        //Instantiate dbContect in Employees Controller
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
        public IActionResult Get()
        {
            //return a list of existing employees from MSSQL
            var result = _dbContext.Employees;
            return Ok(result);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            //Grab the employee details by ID then return result
            var employeeByIdResult = _dbContext.Employees.FirstOrDefault(employee => employee.Id == id);
            if (employeeByIdResult == null) return NotFound();
            return Ok(employeeByIdResult);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and update changes to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(EditEmployeeDto input)
        {
            //Find the employee using ID, then update it's data
            var employeeUpdateData = _dbContext.Employees.Find(input.Id);
            if (employeeUpdateData == null) return NotFound();
            employeeUpdateData.FullName = input.FullName;
            employeeUpdateData.Birthdate = input.Birthdate;
            employeeUpdateData.Tin = input.Tin;
            employeeUpdateData.TypeId = input.TypeId;
            //Save changes then return updated employee object
            _dbContext.SaveChanges();
            return Ok(employeeUpdateData);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and insert employees to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(EmployeeDto input)
        {
            //Add employee then save chanes
            _dbContext.Employees.Add(input);
            _dbContext.SaveChanges();

            //grab the ID of the last added employee then return it
            int lastID = _dbContext.Employees.Max(employee => employee.Id);

            return Created($"/api/employees/{lastID}", lastID);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and perform soft deletion of an employee to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //Find the employee using ID, then update IsDeleted property
            var employeeDeleteData = _dbContext.Employees.Find(id);
            if (employeeDeleteData == null) return NotFound();
            employeeDeleteData.IsDeleted = 1;
            //Save changes
            _dbContext.SaveChanges();
            return Ok(id);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and use Factory pattern
        /// </summary>
        /// <param CalculateClass></param>
        /// <returns></returns>
        [HttpPost("{id}/calculate")]
        public async Task<IActionResult> Calculate([FromBody] CalculateClass calculateObj)
        {
            try
            {
                var employeeData = _dbContext.Employees.Find(calculateObj.id);
                if (employeeData == null) return NotFound();
                var type = (EmployeeType)employeeData.TypeId;
                var totalSalary = calCulateRegularEmployeeSalary(calculateObj.absentDays, calculateObj.workedDays, type);
                return Ok(totalSalary);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private decimal calCulateRegularEmployeeSalary(decimal AbsentDays, decimal workedDays, EmployeeType empType)
        {
            decimal totalSalary = 0;
            switch (empType)
            {
                case EmployeeType.Regular:
                    var daysInMonth = 21;
                    var salary = 20000;
                    var tax = 12;
                    totalSalary = salary - (salary / (daysInMonth + AbsentDays));
                    totalSalary = totalSalary - ((totalSalary * tax) / 100);
                    break;
                case EmployeeType.Contractual:
                    var salaryPerDay = 500;
                    totalSalary = salaryPerDay * workedDays;
                    break;
                default:
                    //Add a retrun for default
                    break;
            }
            return Math.Round(totalSalary, 2);
        }
    }
}
