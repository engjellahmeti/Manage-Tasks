using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Manage_Tasks.Models;

namespace Manage_Tasks.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class HelpController : Controller
    {
        dbManageTasks db = new dbManageTasks();
        

        public string getName(string userId)
        {
            var getSpecificUser = db.UserProfile.FirstOrDefault(x => x.UsID == userId);

            return "" + getSpecificUser.Name + " " + getSpecificUser.Surname;
        }

        public string getJobTitle(string userId)
        {
            var getSpecificUser = db.UserProfile.FirstOrDefault(x => x.UsID == userId);

            return "" + getSpecificUser.JobTitle;
        }


        public ActionResult Manage()
        {
            return View();
        }

        public void giveRolesToUser()
        {
            
        }
    }
}