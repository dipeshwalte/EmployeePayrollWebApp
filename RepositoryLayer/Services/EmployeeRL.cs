using System;
using System.Collections.Generic;
using System.Text;
using RepositoryLayer.Interfaces;
using CommonLayer.ResponseModel;
using System.Data.SqlClient;
using System.Data;

namespace RepositoryLayer.Services
{
    public class EmployeeRL : IEmployeeRL
    {
        string connectionString = @"Data Source=DESKTOP-GBP5KKD\SQLEXPRESS;Initial Catalog=EmployeePayroll;Integrated Security=True;MultipleActiveResultSets=true";
        private List<EmployeeModel> groupEmployees(List<EmployeeModel> inputEmployees)
        {
            List<EmployeeModel> outputEmployees = new List<EmployeeModel>();
            EmployeeModel outputEmployee = new EmployeeModel();
            bool firstFlag = true;
            EmployeeModel previousEmployee = new EmployeeModel();
            foreach (EmployeeModel currentEmployee in inputEmployees)
            {
                if (firstFlag)
                {
                    previousEmployee = currentEmployee;
                    outputEmployee = currentEmployee;
                    firstFlag = false;
                    continue;
                }
                else if (currentEmployee.id == previousEmployee.id)
                {
                    outputEmployee.Department.AddRange(currentEmployee.Department);
                }
                else
                {
                    outputEmployees.Add(outputEmployee);
                    outputEmployee = currentEmployee;
                }
                previousEmployee = currentEmployee;
            }
            outputEmployees.Add(outputEmployee);
            return outputEmployees;
        }
        private List<EmployeeModel> getEmployeeRecords()
        {
            List<EmployeeModel> employees = new List<EmployeeModel>();
            
            SqlConnection connection = new SqlConnection(connectionString);
            string query = "sp_SelectAllRecordsFromEmployeePayroll";
            using (connection)
            {
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                connection.Open();
                SqlDataReader dr = sqlCommand.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        EmployeeModel emp = new EmployeeModel();
                        emp.id = dr.GetInt32(0);
                        emp.Name = dr.GetString(1);
                        emp.Gender = Convert.ToChar(dr.GetString(2));
                        emp.Salary = dr.GetDecimal(3);
                        emp.StartDate = dr.GetDateTime(4);
                        emp.ProfileImage = dr.GetString(5);
                        emp.Notes = dr.GetString(6);
                        emp.Department.Add(dr.GetString(7));
                        employees.Add(emp);
                    }//end while

                }//end if
                connection.Close();
            }//end using
            List<EmployeeModel> departmentGroupedEmployeeList = groupEmployees(employees);
            return departmentGroupedEmployeeList;
        }

        private string insertEmployeeRecord(EmployeeModel employee)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            string storedProcedure = "sp_AddEmployeeRecord";
            string storedProcedureEmpDept = "sp_InsertIntoEmpDept";
            using (connection)
            {
                connection.Open();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Insert Employee Transaction");
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(storedProcedure, connection, transaction);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@StartDate", employee.StartDate);
                    sqlCommand.Parameters.AddWithValue("@EmployeeName", employee.Name);
                    sqlCommand.Parameters.AddWithValue("@Gender", employee.Gender);
                    sqlCommand.Parameters.AddWithValue("@Salary", employee.Salary);
                    sqlCommand.Parameters.AddWithValue("@ProfileImage", employee.ProfileImage);
                    sqlCommand.Parameters.AddWithValue("@Notes", employee.Notes);
                    
                    SqlParameter outPutVal = new SqlParameter("@ScopeIdentifier", SqlDbType.Int);
                    outPutVal.Direction = ParameterDirection.Output;
                    sqlCommand.Parameters.Add(outPutVal);

                    sqlCommand.ExecuteNonQuery();
                    foreach (string department in employee.Department)
                    {
                        SqlCommand sqlCommand1 = new SqlCommand(storedProcedureEmpDept, connection, transaction);
                        sqlCommand1.CommandType = CommandType.StoredProcedure;
                        sqlCommand1.Parameters.AddWithValue("@Emp_id", outPutVal.Value);
                        sqlCommand1.Parameters.AddWithValue("@DepartmentName", department);
                        sqlCommand1.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    connection.Close();
                    return "Data Added Successfully";
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        
                        Console.WriteLine(ex2.Message);
                        return ex2.Message;

                    }
                    return ex.Message;
                }
            }


        }

        private bool deleteEmployeeRecord(int EmpId)
        {
            int recordsAffected = 0;
            SqlConnection connection = new SqlConnection(connectionString);
            string storedProcedure = "sp_DeleteRecordForId";
            try
            {
                using (connection)
                {
                    SqlCommand sqlCommand = new SqlCommand(storedProcedure, connection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@ID", EmpId);
                    connection.Open();
                    recordsAffected += sqlCommand.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return recordsAffected == 0 ? false : true ;

        }

        private List<string> getDepartmentsById(int empId, SqlConnection connection, SqlTransaction transaction)
        {
            List<string> departments = new List<string>();
            string query = "sp_SelectFromEmpDeptbyId";
                SqlCommand sqlCommand = new SqlCommand(query, connection,transaction);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Emp_id", empId);
                SqlDataReader dr = sqlCommand.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        departments.Add(dr.GetString(0));   
                    }//end while
                }//end if
            dr.Close();
            return departments;
        }
        private List<string> getDepartmentsToDelete(List<string> updatedDepartments, List<string> previousDepartments)
        {
            List<string> departmentsToAdd = new List<string>();
            foreach (string updatedDepartment in updatedDepartments)
            {
                bool foundFlag = false;
                foreach (string previousDepartment in previousDepartments)
                {
                    if (previousDepartment==updatedDepartment)
                    {
                        foundFlag = true;
                    }
                }
                if (!foundFlag)
                {
                    departmentsToAdd.Add(updatedDepartment);
                }
            }
            return departmentsToAdd;
        }

        private List<string> getDepartmentsToAdd(List<string> updatedDepartments, List<string> previousDepartments)
        {
            List<string> departmentsToDelete = new List<string>();
            foreach (string previousDepartment in previousDepartments)
            {
                bool foundFlag = false;
                foreach (string updatedDepartment in updatedDepartments)
                {
                    if (previousDepartment == updatedDepartment)
                    {
                        foundFlag = true;
                    }
                }
                if (!foundFlag)
                {
                    departmentsToDelete.Add(previousDepartment);
                }
            }
            return departmentsToDelete;
        }

        private bool updateEmployeeRecord(EmployeeModel employee)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            string storedProcedureUpdateEmployee = "sp_UpdateEmployeeRecord";
            string storedProcedureInsertIntoEmpDept = "sp_InsertIntoEmpDept";
            string storedProcedureDeleteFromEmpDept = "sp_DeleteFromEmpDept";
            using (connection)
            {
                connection.Open();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("Update Employee Transaction");
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(storedProcedureUpdateEmployee, connection, transaction);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@StartDate", employee.StartDate);
                    sqlCommand.Parameters.AddWithValue("@EmployeeName", employee.Name);
                    sqlCommand.Parameters.AddWithValue("@Gender", employee.Gender);
                    sqlCommand.Parameters.AddWithValue("@Salary", employee.Salary);
                    sqlCommand.Parameters.AddWithValue("@ProfileImage", employee.ProfileImage);
                    sqlCommand.Parameters.AddWithValue("@Notes", employee.Notes);
                    sqlCommand.ExecuteNonQuery();
                    List<string> updatedDepartments = getDepartmentsById(employee.id,connection,transaction);
                    List<string> departmentsToAdd = getDepartmentsToAdd(updatedDepartments, employee.Department);
                    List<string> departmentsToDelete = getDepartmentsToDelete(updatedDepartments, employee.Department);
                    foreach (string department in departmentsToAdd)
                    {
                        SqlCommand sqlCommand1 = new SqlCommand(storedProcedureInsertIntoEmpDept, connection, transaction);
                        sqlCommand1.CommandType = CommandType.StoredProcedure;
                        sqlCommand1.Parameters.AddWithValue("@Emp_id", employee.id);
                        sqlCommand1.Parameters.AddWithValue("@DepartmentName", department);
                        sqlCommand1.ExecuteNonQuery();
                    }
                    foreach (string department in departmentsToDelete)
                    {
                        SqlCommand sqlCommand1 = new SqlCommand(storedProcedureDeleteFromEmpDept, connection, transaction);
                        sqlCommand1.CommandType = CommandType.StoredProcedure;
                        sqlCommand1.Parameters.AddWithValue("@Emp_id", employee.id);
                        sqlCommand1.Parameters.AddWithValue("@DepartmentName", department);
                        sqlCommand1.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    connection.Close();
                    return true;
                }

                catch (Exception ex)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        return false;
                    }
                    return false;
                }
            }


        }

        public List<EmployeeModel> ReturnEmployeeRecords()
        {
            return getEmployeeRecords();
        }

        public string AddEmployeeRecord(EmployeeModel employee)
        {
            return insertEmployeeRecord(employee);
        }

        public bool DeleteEmployeeRecord(int EmpId)
        {
            return deleteEmployeeRecord(EmpId);
        }

        public bool UpdateEmployeeRecord(EmployeeModel employee)
        {
            return updateEmployeeRecord(employee);
        }
    }
}
