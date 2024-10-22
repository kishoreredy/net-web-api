using CodeFirstApi.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeFirstApi.Models
{
    [Keyless]
    public class Salary
    {
        public long EmployeeId { get; set; }
        
        public double Ctc {  get; set; }

        public Department Department { get; set; }

        public EmployeeType EmployeeType { get; set; }

        public double AnnualBonus { get; set; }


        [ForeignKey("EmployeeId")]
        public virtual required Employee User { get; set; }
    }
}
