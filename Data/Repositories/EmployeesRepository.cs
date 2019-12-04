

using System.Linq;
using ent.manager.Data.Context;
using ent.manager.Entity.Model;

namespace ent.manager.Data.Repositories
{
    public class EmployeesRepository : IRepository<Employee>
    {

        private EmployeesContext db;

        public EmployeesRepository(EmployeesContext dbContext)
        {
            db = dbContext;
        }

        public Employee CreateEmployee(Employee employee)
        {
           

            db.Employees.Add(employee);
            db.SaveChanges();
            // Now we generate a Base36 string for the DB id:
    
            db.SaveChanges();
            return employee;
        }

        public Employee GetEmployeeById(int id)
        {
            Employee employee = db.Employees.First(s => s.id == id);
            return employee;
        }

        
 
    }
}
