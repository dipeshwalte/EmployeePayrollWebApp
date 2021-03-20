using CommonLayer.ResponseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLayer.Interfaces
{
    public interface IEmployeeBL
    {
        public List<EmployeeModel> ReturnEmployeeRecords();
        public string RegisterRecord(EmployeeModel employee);
        public bool DeleteEmployeeRecord(int EmpId);

        public bool UpdateEmployeeRecord(EmployeeModel employee);
    }
}
