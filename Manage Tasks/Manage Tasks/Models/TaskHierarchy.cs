using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Manage_Tasks.Models
{
    public class TaskHierarchy
    {
        public int ID { set; get; }
        public string Name { get; set; }
        public int? ManagerID { set; get; }
        public IList<TaskHierarchy> Users { set; get; }
        public TaskHierarchy()
        {
            Users = new List<TaskHierarchy>();
        }
    }
}