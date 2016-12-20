﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ManageEmployees.Data.Abstract;
using ManageEmployees.Data.Repositories;
using ManageEmployees.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManageEmployees.Controllers
{
    [Route("api/[controller]")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DepartmentController(IDepartmentRepository departmentRepository, IEmployeeRepository employeeRepository)
        {
            this._departmentRepository = departmentRepository;
            this._employeeRepository = employeeRepository;
        }
        

        [HttpGet]
        public IActionResult Get()
        {
            //return Ok("Ervis");
            IQueryable<Department> departments = _departmentRepository.AllIncluding(emp=>emp.Employees);
            
            if (departments.Any())
                return Ok(departments);
            return NotFound();
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Department department = _departmentRepository.GetSingle(p => p.Id == id, e=>e.Employees);

            if (department != null)
                return Ok(department);
            return NotFound();
        }

        [HttpGet("{id}/employees")]
        public IActionResult GetDepartmentEmployees(int id)
        {
            var departmentEmployees = _employeeRepository.AllIncluding(c=>c.Contracts).Where(dep => dep.DepartmentId == id);

            if (departmentEmployees != null)
                return Ok(departmentEmployees);
            return NotFound();
        }

        [HttpPost]
        public IActionResult Post([FromBody] Department department)
        {
            try
            {
                var _department = department;
                if (_department == null) throw new ArgumentNullException(nameof(_department));
                _departmentRepository.Add(_department);
                _departmentRepository.Commit();

                return Created($"/api/department/{department.Id}", _department);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Department department)
        {
            try
            {
                var _department = _departmentRepository.GetSingle(id);
                if (_department == null) throw new ArgumentNullException(nameof(_department));
                _department.Name = department.Name;
                _departmentRepository.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var department = _departmentRepository.GetSingle(id);
            if (department == null) throw new ArgumentNullException(nameof(department));

            var departmentEmployees = _employeeRepository.FindBy(a => a.DepartmentId == id);

            if (departmentEmployees != null) return BadRequest("Department you want to delete has employees assigned.");

            _departmentRepository.Delete(department);
            _departmentRepository.Commit();
            
            return new NoContentResult();
        }
    }
}