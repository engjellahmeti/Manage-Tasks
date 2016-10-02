using Manage_Tasks.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Manage_Tasks.Controllers
{
    [Authorize]
    public class ArchiveController : Controller
    {
        dbManageTasks db = new dbManageTasks();
        // GET: Archive
        public ActionResult Index()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                return View(db.TaskConfig.Where(x => x.IsCompleted == true).OrderByDescending(x => x.DateWhenCompleted).ToList());
            }
            else
            {
                var getUserGroup = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);
                List<TaskConfig> users = returnTasksValid(int.Parse(getUserGroup.GroupID.ToString()));
                return View(users);
            }
        }


        [HttpPost]
        public ActionResult Index(IEnumerable<TaskConfig> task)
        {
            try
            {
                var getM = Request["getTaskId"].Split(',').ToList();
                List<int> t = new List<int>();
                foreach (var m in getM)
                {
                    if (m == "")
                    {
                        continue;
                    }
                    else
                    {
                        int id = int.Parse(m.ToString());
                        t.Add(id);
                    }
                }
                var model = db.TaskConfig.Where(x => t.Contains(x.TaskID));
                string getTasks = Request["KeywordTasks"].ToString();
                //var tasksEnd = db.TaskConfig.Where(x => x.TaskID == 0);
                if (getTasks != null && getTasks != "")
                {
                    var tasks = model.Where(x => x.Keywords.Contains(getTasks)).OrderByDescending(x => x.TaskEndDate).ToList();
                    if (tasks.Count() == 1)
                    {
                        int id = model.FirstOrDefault(x => x.Keywords.Contains(getTasks)).TaskID;
                        //int id = modeli.FirstOrDefault(x => x.Keywords.Contains(getTasks)).TaskID;
                        return RedirectToAction("returnTaskSelected", "TaskConfigs", new { id = id });
                    }
                    else if (tasks.Count() > 1)
                    {
                        return View(tasks.ToList());
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Archive");
                }
                //return View(tasksEnd.ToList());
            }
            catch (DataException ex)
            {
                throw ex;
            }
            return View();
        }



        public List<TaskConfig> returnTasksValid(int GroupId)
        {
            var getSuperAdmin = db.UserProfile.FirstOrDefault(x => x.Username == "engjellahmeti");
            List<TaskConfig> listaEUserave = new List<TaskConfig>();
            //var kk = db.AspNetUsers.Where(x => ))
            switch (GroupId)
            {
                case 1:
                    listaEUserave = db.TaskConfig.Where(x => x.CreatorsGroupId != null && x.IsCompleted == true).OrderByDescending(x => x.TaskEndDate).ToList();
                    break;
                case 2:
                    listaEUserave = db.TaskConfig.Where(x => x.CreatorsGroupId != null && x.CreatorsGroupId != 1 && x.IsCompleted == true).OrderByDescending(x => x.TaskEndDate).ToList();
                    break;
                case 3:
                    listaEUserave = db.TaskConfig.Where(x => x.CreatorsGroupId != null && x.CreatorsGroupId != 1 && x.CreatorsGroupId != 2).OrderByDescending(x => x.TaskEndDate).ToList();
                    break;
                case 4:
                    listaEUserave = db.TaskConfig.Where(x => x.CreatorsGroupId != null && x.CreatorsGroupId != 1 && x.CreatorsGroupId != 2 && x.CreatorsGroupId != 3 && x.IsCompleted == true).OrderByDescending(x => x.TaskEndDate).ToList();
                    break;
                case 5:
                    listaEUserave = db.TaskConfig.Where(x => x.CreatorsGroupId != null && x.CreatorsGroupId != 1 && x.CreatorsGroupId != 2 && x.CreatorsGroupId != 3 && x.CreatorsGroupId != 4 && x.IsCompleted == true).OrderByDescending(x => x.TaskEndDate).ToList();
                    break;

            }
            return listaEUserave;
        }

        [HttpGet]
        public ActionResult taskAssigned()
        {
            var getId = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);
            var getTasks = db.TaskConfig.Where(x => x.TaskID == x.TaskCreatorUser.FirstOrDefault(t => t.UserID == getId.UserID).TaskID && x.IsCompleted == true).OrderByDescending(x => x.TaskEndDate).ToList();
            return View(getTasks);
        }


        [HttpPost]
        public ActionResult taskAssigned(FormCollection form)
        {
            try
            {
                var getM = Request["getTaskId"].Split(',').ToList();
                List<int> t = new List<int>();
                foreach (var m in getM)
                {
                    if (m == "")
                    {
                        continue;
                    }
                    else
                    {
                        int id = int.Parse(m.ToString());
                        t.Add(id);
                    }
                }
                var model = db.TaskConfig.Where(x => t.Contains(x.TaskID));
                string getTasks = Request["KeywordTasks"].ToString();                
                if (getTasks != null && getTasks != "")
                {
                    var tasks = model.Where(x => x.Keywords.Contains(getTasks)).OrderByDescending(x => x.TaskEndDate).ToList();
                    if (tasks.Count() == 1)
                    {
                        int id = model.FirstOrDefault(x => x.Keywords.Contains(getTasks)).TaskID;
                        //int id = modeli.FirstOrDefault(x => x.Keywords.Contains(getTasks)).TaskID;
                        return RedirectToAction("returnTaskSelected", "TaskConfigs", new { id = id });
                    }
                    else if (tasks.Count() > 1)
                    {
                        return View(tasks.ToList());
                    }
                }
                else
                {
                    return RedirectToAction("taskAssigned", "Archive");
                }
              }
            catch (DataException ec)
            {
                throw ec;
            }
            return View();

        }


        [HttpPost]
        public JsonResult Keywords(string Prefix, string res)
        {
            var getM = res.Split(',').ToList();
            List<int> t = new List<int>();
            foreach (var m in getM)
            {
                if (m == "")
                {
                    continue;
                }
                else
                {
                    int id = int.Parse(m.ToString());
                    t.Add(id);
                }
            }
            var model = db.TaskConfig.Where(x => t.Contains(x.TaskID));
            List<SelectTempTask> listaETaskave = new List<SelectTempTask>();
            var user5 = (from x in model
                         where x.Keywords.Contains(Prefix)
                         select new { x.Keywords, x.TaskID });
            foreach (var key in user5)
            {
                SelectTempTask s1 = new SelectTempTask
                {
                    Keywords = key.Keywords,
                    TaskID = key.TaskID
                };
                listaETaskave.Add(s1);
            }

            return Json(listaETaskave, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Keywordat(string Prefix, string res)
        {
            var getM = res.Split(',').ToList();
            List<int> t = new List<int>();
            foreach (var m in getM)
            {
                if (m == "")
                {
                    continue;
                }
                else
                {
                    int id = int.Parse(m.ToString());
                    t.Add(id);
                }
            }
            var model = db.TaskConfig.Where(x => t.Contains(x.TaskID));
            List<SelectTempTask> listaETaskave = new List<SelectTempTask>();
            var user5 = (from x in model
                                 where x.Keywords.Contains(Prefix)
                                 select new { x.Keywords, x.TaskID });
            foreach (var key in user5)
            {
                SelectTempTask s1 = new SelectTempTask
                {
                    Keywords = key.Keywords,
                    TaskID = key.TaskID
                };
                listaETaskave.Add(s1);
            }
           

            return Json(listaETaskave, JsonRequestBehavior.AllowGet);
        }


    }
    public class SelectTempTask
    {
        public int TaskID { get; set; }
        public string Keywords { get; set; }
    }
}