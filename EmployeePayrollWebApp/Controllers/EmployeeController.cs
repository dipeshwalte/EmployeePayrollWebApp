using BusinessLayer.Interfaces;
using CommonLayer.ResponseModel;
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
    public class EmployeeController : ControllerBase
    {
        IEmployeeBL employeeBL;
        public EmployeeController(IEmployeeBL employeeBL)
        {
            this.employeeBL = employeeBL;
        }
        [HttpGet]
        public ActionResult ReturnEmployeeRecords()
        {
            try
            {
                List<EmployeeModel> employees = this.employeeBL.ReturnEmployeeRecords();
                return this.Ok(new { Success = "True", Message = "Successfully Retrieved Data", data = employees });
            }
            catch (Exception ex)
            {
                return this.BadRequest(new { Success = "False", Message = ex.Message });
            }
            
        }
        [HttpPost("Register")]
        public ActionResult PostEmployeeData(EmployeeModel employee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string resultMessage = this.employeeBL.RegisterRecord(employee);
                    return this.Ok(new { Success = true, Message = resultMessage });
                }
                else
                {
                    throw new Exception("Model is Invalid");
                }
                
            }
            catch (Exception ex)
            {
                return this.BadRequest(new { Success = false, Message = ex.Message });
            }
        }
        [HttpPut("Update")]
        public ActionResult PutEmployeeData(EmployeeModel employee)
        {
            try
            {
                 bool successFlag = this.employeeBL.UpdateEmployeeRecord(employee);
                if (successFlag)
                {
                    return this.Ok(new { Success = successFlag, Message = "Record Updation Successful" });
                }
                else
                {
                    return this.Ok(new { Success = successFlag, Message = "Record Updation Unsuccessful" });
                }
                 
            }
            catch (Exception ex)
            {
                return this.BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("Delete/{Emp_id}")]
        public ActionResult DeleteEmployeeRecord(int Emp_id)
        {
            try
            { 
                bool successfulExecution = this.employeeBL.DeleteEmployeeRecord(Emp_id);
                if (successfulExecution)
                {
                    return this.Ok(new { Success = true, Message = "Successfully Deleted Record" });
                }
                else
                {
                    return this.Ok(new { Success = false, Message = "Record not found" });
                }
                
            }
            catch (Exception ex)
            {
                return this.BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
