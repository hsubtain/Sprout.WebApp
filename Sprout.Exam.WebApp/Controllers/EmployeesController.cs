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
        /// Retrieve all active employees from the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                //return a list of existing employees from MSSQL
                var result = _dbContext.Employees;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        /// <summary>
        /// Retrieve a specific employee using the employee's ID
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                //Grab the employee details by ID then return result
                var employeeByIdResult = _dbContext.Employees.FirstOrDefault(employee => employee.Id == id);
                if (employeeByIdResult == null) return NotFound();
                return Ok(employeeByIdResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        /// <summary>
        /// Update an employee information using 
        /// </summary>
        /// <returns><c>EmployeeDto<c></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(EditEmployeeDto input)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        /// <summary>
        /// Add new employee to the database
        /// Return the ID of the new employee
        /// </summary>
        /// <returns><c>EmployeeDto.Id.<c></returns>
        [HttpPost]
        public async Task<IActionResult> Post(EmployeeDto input)
        {
            try
            {
                //Add employee then save chanes
                _dbContext.Employees.Add(input);
                _dbContext.SaveChanges();

                //grab the ID of the last added employee then return it
                int lastID = _dbContext.Employees.Max(employee => employee.Id);

                return Created($"/api/employees/{lastID}", lastID);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        /// <summary>
        /// Delete an employee from the database
        /// This will perform a soft delete, ths isDeleted property will be modified
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                //Find the employee using ID, then update IsDeleted property
                var employeeDeleteData = _dbContext.Employees.Find(id);
                if (employeeDeleteData == null) return NotFound();
                employeeDeleteData.IsDeleted = 1;
                //Save changes
                _dbContext.SaveChanges();
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Perform a calculation for the employee's salary based on it's type
        /// </summary>
        /// <param><c>CalculateClass</c>c></param>
        /// <returns><c>Total Salary</c>c></returns>
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

        /// <summary>
        /// Function for performing the calculation on the employee's salary
        /// We can handle future employee type by adding to the EmployeeType enum class
        /// Add switch case for the newly added employee type
        ///<returns><c>Total Salary computed</c>c></returns>
        /// </summary>
        private decimal calCulateRegularEmployeeSalary(decimal AbsentDays, decimal workedDays, EmployeeType empType)
        {
            decimal totalSalary = 0;
            switch (empType)
            {
                case EmployeeType.Regular:
                    var daysInMonth = 21;
                    var salary = 20000;
                    var tax = 12;
                    totalSalary = salary - ((salary / daysInMonth) * AbsentDays);
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
