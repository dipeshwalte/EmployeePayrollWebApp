using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CommonLayer.ResponseModel
{
   public class EmployeeModel
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public char Gender { get; set; }
        [Required]
        public decimal Salary { get; set; }
        [Required]
        public string Notes { get; set; }
        [Required]
        public string ProfileImage { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public List<string> Department { get; set; }

        public EmployeeModel()
        {
            Department = new List<string>();
        }
        
    }

   
}
