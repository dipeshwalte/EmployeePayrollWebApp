using System;
using System.Collections.Generic;
using System.Text;
using CommonLayer.ResponseModel;

namespace RepositoryLayer.Interfaces
{
    public interface IEmployeeRL
    {
        public List<EmployeeModel> ReturnEmployeeRecords();

        public string AddEmployeeRecord(EmployeeModel employee);

        public bool DeleteEmployeeRecord(int EmpId);

        public bool UpdateEmployeeRecord(EmployeeModel employee);
    }
}
