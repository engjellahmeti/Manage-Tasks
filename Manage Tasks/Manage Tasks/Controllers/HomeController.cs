using Manage_Tasks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Manage_Tasks.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        dbManageTasks db = new dbManageTasks();

        public ActionResult Index()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                return View(db.TaskConfig.Where(x => x.IsCompleted == false).OrderByDescending(x => x.CreatedOnDate).ToList());
            }
            else
            {
                var getUserId = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);
                var getTasksId = db.TaskCreatorUser.Where(x => x.UserID == getUserId.UserID).Select(x => x.TaskID);
                List<TaskConfig> tc = new List<TaskConfig>();
                var getTasksCreatedByThisUser = db.TaskConfig.Where(x => x.CreatedByUserId == db.AspNetUsers.FirstOrDefault(t => t.UserName == User.Identity.Name).Id && x.IsCompleted == false).OrderByDescending(x => x.TaskEndDate);
                foreach (var id in getTasksId)
                {
                    TaskConfig t = db.TaskConfig.Find(id);
                    if (t.IsCompleted == false)
                    {
                        tc.Add(t);
                    }
                    else
                    {
                        continue;
                    }

                }
                foreach (TaskConfig t in getTasksCreatedByThisUser)
                {
                    if (tc.Contains(t))
                    {
                        continue;
                    }
                    else
                    {
                        tc.Add(t);
                    }
                }
                return View(tc.OrderByDescending(x => x.CreatedOnDate));
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult GetUsers(string Prefix)
        {
            var getUsers = db.UserProfile.Where(x => x.FullName.Contains(Prefix)).Select(x => x.UserID);

            return Json(getUsers, JsonRequestBehavior.AllowGet);
        }
    }
}