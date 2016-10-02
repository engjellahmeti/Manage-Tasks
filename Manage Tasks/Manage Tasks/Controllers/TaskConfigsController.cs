using System;
using System.Collections.Generic;
//using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Data;
using Manage_Tasks.Models;

namespace Manage_Tasks.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin, Nenadmin, Personel Akademik-Asistent, Personel Akademik-Ligjerues, Staf")]
    public class TaskConfigsController : Controller
    {
        private dbManageTasks db = new dbManageTasks();
        ApplicationDbContext context = new ApplicationDbContext();

        // GET: TaskConfigs
        public ActionResult Index()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                return View(db.TaskConfig.OrderByDescending(x => x.CreatedOnDate).ToList());
            }
            else
            {
                var getUserId = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);
                var getTasksId = db.TaskCreatorUser.Where(x => x.UserID == getUserId.UserID).Select(x => x.TaskID);
                List<TaskConfig> tc = new List<TaskConfig>();
                var getTasksCreatedByThisUser = db.TaskConfig.Where(x => x.CreatedByUserId == db.AspNetUsers.FirstOrDefault(t => t.UserName == User.Identity.Name).Id );
                //foreach (var id in getTasksId)
                //{
                //    TaskConfig t = db.TaskConfig.Find(id);
                //    tc.Add(t);
                //}
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

        // GET: TaskConfigs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskConfig t = db.TaskConfig.Find(id);
            var userList = db.TaskCreatorUser.Where(x => x.TaskID == id).ToList();
            List<TaskHierarchy> emp = new List<TaskHierarchy>();
            bool pres = false;
            foreach (var ul in userList)
            {
                var name1 = db.UserProfile.FirstOrDefault(x => x.UserID == ul.CreatorID).FullName;
                var name = db.UserProfile.FirstOrDefault(x => x.UserID == ul.UserID).FullName;

                if (pres == false)
                {
                    if (ul.CreatorID != null)
                    {
                        string i = "" + ul.CreatorID;
                        TaskHierarchy e = new TaskHierarchy
                        {
                            ID = int.Parse(i),
                            Name = name1,
                            ManagerID = null
                        };
                        emp.Add(e);
                        TaskHierarchy e1 = new TaskHierarchy
                        {
                            ID = ul.UserID,
                            Name = name,
                            ManagerID = ul.CreatorID
                        };
                        emp.Add(e1);
                    }
                    else
                    {

                    }
                }
                else
                {

                    TaskHierarchy e = new TaskHierarchy
                    {
                        ID = ul.UserID,
                        Name = name,
                        ManagerID = ul.CreatorID
                    };
                    emp.Add(e);
                }
            }
            ViewBag.DocumentsOfTask = db.DocumentsCreated.Where(x => x.TaskID == id && x.UserID == null).OrderByDescending(x => x.CreatedOnDate).ToList();
            var president = emp.FirstOrDefault(x => x.ManagerID == null);
            SetChildren(president, emp);
            ViewBag.Users = president;
            return View(t);
        }

        // GET: TaskConfigs/Create
        public ActionResult Create()
        {
            try
            {                
                if (User.IsInRole("SuperAdmin"))
                {
                    ViewBag.peopleListed = new MultiSelectList(db.UserProfile.ToList(), "UserID", "FullName");
                }
                else {
                    var getUserGroup = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);
                    List<UserProfile> users = returnUsersValid(int.Parse(getUserGroup.GroupID.ToString()));
                    ViewBag.peopleListed = new MultiSelectList(users, "UserID", "FullName");

                }
                return View();
            }
            catch
            {

            }
            return View();
        }

        // POST: TaskConfigs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TaskName, TaskDescription, TaskBeginDate,TaskEndDate, Keywords, LastModifiedByUserID, CreatedByUserId")] TaskConfig taskConfig, HttpPostedFileBase file)
        {
            var UsersId = Request["takeUsersID"].Split(',').ToList();
            if (ModelState.IsValid)
            {
                var getUser = db.UserProfile.FirstOrDefault(x => x.UsID == taskConfig.CreatedByUserId);
                try
                {
                    TaskConfig t = new TaskConfig()
                    {
                        TaskName = taskConfig.TaskName,
                        TaskDescription = taskConfig.TaskDescription,
                        TaskBeginDate = taskConfig.TaskBeginDate + DateTime.Now.TimeOfDay,
                        TaskEndDate = taskConfig.TaskEndDate + DateTime.Now.TimeOfDay,
                        Keywords = taskConfig.Keywords,
                        IsAccepted = false,
                        CreatorsGroupId = getUser.GroupID,
                        Pending = false,
                        IsCompleted = false,
                        LastModifiedOnDate = DateTime.Now,
                        CreatedOnDate = DateTime.Now,
                        CreatedByUserId = taskConfig.CreatedByUserId,
                        LastModifiedByUserID = taskConfig.LastModifiedByUserID
                    };
                    db.TaskConfig.Add(t);
                    db.SaveChanges();
                }
                catch (DataException ex)
                {

                }
                try
                {
                    int getThisTaskId = db.TaskConfig.Max(item => item.TaskID);
                    var getUserId = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);

                    if (file != null)
                    {
                        DocumentsCreated dc = new DocumentsCreated();
                        //dc.File = file;
                        dc.FileName = file.FileName;
                        dc.FileSize = file.ContentLength;

                        byte[] imageBytes = null;
                        BinaryReader reader = new BinaryReader(file.InputStream);
                        imageBytes = reader.ReadBytes((int)file.ContentLength);
                        dc.Dokument = imageBytes;
                        dc.TaskID = getThisTaskId;
                        dc.IsDeleted = false;
                        dc.CreatedOnDate = DateTime.Now;
                        dc.LastModifiedByUserID = getUserId.UsID;
                        dc.LastModifiedOnDate = DateTime.Now;
                        db.DocumentsCreated.Add(dc);
                        db.SaveChanges();
                    }
                    foreach (var id in UsersId)
                    {
                        int i = int.Parse(id.ToString());
                        var name = db.UserProfile.FirstOrDefault(x => x.UserID == i);
                        TaskCreatorUser tu = new TaskCreatorUser
                        {
                            TaskID = getThisTaskId,
                            UserID = int.Parse(id.ToString()),
                            CreatorID = int.Parse(getUserId.UserID.ToString()),
                            Name = name.FullName.ToString()
                        };
                        db.TaskCreatorUser.Add(tu);
                    }
                    db.SaveChanges();
                }
                catch (DataException ex)
                {

                }
                return RedirectToAction("Index");
            }

            return View(taskConfig);
        }

        [HttpGet]
        // GET: TaskConfigs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskConfig taskConfig = db.TaskConfig.Find(id);
            //var t = db.TaskCreatorUser.Where(x => x.TaskID == id).Select(x => x.UserID);
            //List<UserProfile> up = new List<UserProfile>();
            //foreach(var upk in t)
            //{
            //    int parso = int.Parse(upk.ToString());
            //    UserProfile up1 = db.UserProfile.Find(parso);
            //    up.Add(up1);
            //}
            if (User.IsInRole("SuperAdmin"))
            {
                ViewBag.peopleListed = new MultiSelectList(db.UserProfile.ToList(), "UserID", "FullName", taskConfig.TaskCreatorUser.Select(x => x.UserID));
            }
            else
            {
                var getUserGroup = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);
                List<UserProfile> users = returnUsersValid(int.Parse(getUserGroup.GroupID.ToString()));
                ViewBag.peopleListed = new MultiSelectList(users, "UserID", "FullName", taskConfig.TaskCreatorUser.Select(x => x.UserID));

            }           
            ViewBag.GetDocument = db.DocumentsCreated.Where(x => x.TaskID == id && x.UserID == null && x.IsDeleted == false).OrderByDescending(x => x.CreatedOnDate).ToList();

            if (taskConfig == null)
            {
                return HttpNotFound();
            }
            return View(taskConfig);
        }

        // POST: TaskConfigs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TaskID,TaskName, TaskDescription, TaskBeginDate,TaskEndDate, Keywords, LastModifiedByUserID")] TaskConfig taskConfig, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                var UsersId = Request["takeUsersID"].Split(',').ToList();

                TaskConfig t1 = db.TaskConfig.Find(taskConfig.TaskID);
                TaskConfig t2 = db.TaskConfig.Find(taskConfig.TaskID);
                t2.TaskName = taskConfig.TaskName;
                t2.TaskDescription = taskConfig.TaskDescription;
                t2.Keywords = taskConfig.Keywords;
                t2.TaskBeginDate = taskConfig.TaskBeginDate;
                t2.TaskEndDate = taskConfig.TaskEndDate;
                t2.LastModifiedByUserID = taskConfig.LastModifiedByUserID;
                db.Entry(t1).CurrentValues.SetValues(t2);
                db.Entry(t1).State = EntityState.Modified;
                db.SaveChanges();
                var getUserId = db.UserProfile.FirstOrDefault(x => x.Username == User.Identity.Name);

                if (file != null)
                {
                    DocumentsCreated dc = new DocumentsCreated();
                    //dc.File = file;
                    dc.FileName = file.FileName;
                    dc.FileSize = file.ContentLength;

                    byte[] imageBytes = null;
                    BinaryReader reader = new BinaryReader(file.InputStream);
                    imageBytes = reader.ReadBytes((int)file.ContentLength);
                    dc.Dokument = imageBytes;
                    dc.TaskID = taskConfig.TaskID;
                    dc.IsDeleted = false;
                    dc.CreatedOnDate = DateTime.Now;
                    dc.LastModifiedByUserID = getUserId.UsID;
                    dc.LastModifiedOnDate = DateTime.Now;
                    db.DocumentsCreated.Add(dc);
                    db.SaveChanges();
                }
                var getCreators = db.TaskCreatorUser.Where(x => x.TaskID == taskConfig.TaskID).ToList();
                foreach(var gC in getCreators)
                {
                    TaskCreatorUser t = db.TaskCreatorUser.Find(gC.ID);
                    db.TaskCreatorUser.Remove(t);
                }
                db.SaveChanges();

                foreach (var id in UsersId)
                {
                    int i = int.Parse(id.ToString());
                    var name = db.UserProfile.FirstOrDefault(x => x.UserID == i);
                    TaskCreatorUser tu = new TaskCreatorUser
                    {
                        TaskID = taskConfig.TaskID,
                        UserID = int.Parse(id.ToString()),
                        CreatorID = int.Parse(getUserId.UserID.ToString()),
                        Name = name.FullName.ToString()
                    };
                    db.TaskCreatorUser.Add(tu);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taskConfig);
        }

        // GET: TaskConfigs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskConfig taskConfig = db.TaskConfig.Find(id);
            if (taskConfig == null)
            {
                return HttpNotFound();
            }
            
            return View(taskConfig);
        }

        // POST: TaskConfigs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var getDocs = db.DocumentsCreated.Where(x => x.TaskID == id).ToList();
            foreach (var d in getDocs)
            {
                DocumentsCreated d1 = db.DocumentsCreated.Find(d.ID);
                db.DocumentsCreated.Remove(d1);
            }
            db.SaveChanges();
            var getCreators = db.TaskCreatorUser.Where(x => x.TaskID == id).ToList();
            foreach (var gC in getCreators)
            {
                TaskCreatorUser k = db.TaskCreatorUser.Find(gC.ID);
                db.TaskCreatorUser.Remove(k);
            }
            db.SaveChanges();
            var getRequestTime = db.RequestTime.Where(x => x.TaskID == id).Select(x => x.ID);
            foreach(var x in getRequestTime)
            {
                int i = int.Parse(x.ToString());
                RequestTime r = db.RequestTime.Find(i);
                db.RequestTime.Remove(r);
            }
            db.SaveChanges();
            TaskConfig t = db.TaskConfig.Find(id);
            db.TaskConfig.Remove(t);
            db.SaveChanges();
            
            

            return RedirectToAction("Index");
        }

        //[HttpPost]
        //public ActionResult UploadDocuments(HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
        //        {
        //            //var pic = System.Web.HttpContext.Current.Request.Files["HelpSectionImages"];
        //            //HttpPostedFileBase file = new HttpPostedFileWrapper(pic);
        //            //var t = collection["taskId"];
        //            var taskId = Request["taskId"].ToString();
        //            var userId = Request["userId"].ToString();
        //            int userid = int.Parse(userId);
        //            int taskid = int.Parse(taskId);
        //            var UId = db.UserProfile.FirstOrDefault(x => x.UserID == userid);
        //            DocumentsCreated dc = new DocumentsCreated();
        //            dc.File = file;
        //            dc.FileName = file.FileName;
        //            dc.FileSize = file.ContentLength;

        //            byte[] imageBytes = null;

        //            BinaryReader reader = new BinaryReader(file.InputStream);
        //            imageBytes = reader.ReadBytes((int)file.ContentLength);
        //            dc.Dokument = imageBytes;
        //            String uid = "" + UId.UserID;
        //            int imgUserId = int.Parse(uid);
        //            dc.UserID = imgUserId;
        //            dc.TaskID = taskid;
        //            dc.IsDeleted = false;
        //            dc.CreatedOnDate = DateTime.Now;
        //            dc.LastModifiedByUserId = UId.UsID;
        //            dc.LastModifiedOnDate = DateTime.Now;
        //            db.DocumentsCreated.Add(dc);
        //            db.SaveChanges();

        //            var listaEDokumenteve = db.DocumentsCreated.Where(x => x.TaskID == taskid && x.UserID == userid && x.IsDeleted == false).ToList();
        //            return PartialView("returnDocumentsCreated", listaEDokumenteve);
        //            // return RenderPartial("FullName", new { firstName = model.FirstName, lastName = model.LastName });
        //        }

        //    }
        //    catch (DataException ex)
        //    {
        //        throw ex;
        //    }


        //    return Json(null, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult returnDocumentsCreated(int TaskID, int UserID)
        {
           var listaEDokumenteve = db.DocumentsCreated.Where(x => x.TaskID == TaskID && x.UserID == UserID && x.IsDeleted == false).OrderByDescending(x => x.CreatedOnDate).ToList();
            var t = db.TaskConfig.Find(TaskID);
            if (Request.IsAjaxRequest())
            {
                ViewBag.user = UserID;
                ViewBag.task = TaskID;
                ViewBag.IsCompleted = t.IsCompleted;
                ViewBag.EndDate = t.TaskEndDate;

                return PartialView("returnDocumentsCreated", listaEDokumenteve);
            }
            else
            {
                return PartialView("returnDocumentsCreated", null);

            }
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public void acceptTask(bool accepted, int id, int acceptFrom)
        {
            try
            {
                TaskConfig t = db.TaskConfig.Find(id);
                TaskConfig t1 = db.TaskConfig.Find(id);
                //var getId = db.UserProfile.FirstOrDefault(x => x.UserID == acceptFrom);
                t1.IsAccepted = accepted;
                t1.AcceptedByUserId = acceptFrom;
                t1.DateWhenAccepted = DateTime.Now;

                db.Entry(t).CurrentValues.SetValues(t1);
                db.Entry(t).State = EntityState.Modified;
                db.SaveChanges();

            }
            catch (DataException ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult completedTask(bool completed, int id, int completedFrom)
        {
            try
            {
                TaskConfig t = db.TaskConfig.Find(id);
                TaskConfig t1 = db.TaskConfig.Find(id);
                //var getId = db.UserProfile.FirstOrDefault(x => x.Username == completedFrom);

                t1.IsCompleted = completed;
                t1.CompletedByUserId = completedFrom;
                t1.DateWhenCompleted = DateTime.Now;

                db.Entry(t).CurrentValues.SetValues(t1);
                db.Entry(t).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            catch (DataException ex)
            {
                throw ex;
                return View();
            }
        }
        

        [HttpPost]
        public void requestTimeWritten(DateTime timeRequsted, int id, int requestedFrom)
        {
            try
            {
                TaskConfig t = db.TaskConfig.Find(id);
                TaskConfig t1 = db.TaskConfig.Find(id);
                t1.Pending = true;
                db.Entry(t).CurrentValues.SetValues(t1);
                db.Entry(t).State = EntityState.Modified;
                db.SaveChanges();
                var getId = db.UserProfile.FirstOrDefault(x => x.UserID == requestedFrom);

                // t1.TaskEndDate = timeRequsted + DateTime.Now.TimeOfDay;
                RequestTime rt = new RequestTime
                {
                    PersonWhoRequested = getId.FullName,
                    DateRequired = t.TaskEndDate,
                    DateRequested = timeRequsted,
                    IsAccepted = false,
                    TaskID = id
                };
                db.RequestTime.Add(rt);
                db.SaveChanges();
                //db.Entry(t).CurrentValues.SetValues(t1);
                //db.Entry(t).State = EntityState.Modified;
                //db.SaveChanges();


            }
            catch (DataException ex)
            {
                throw ex;
            }
        }

        public ActionResult returnTaskSelected(int id)
        {

            TaskConfig t = db.TaskConfig.Find(id);
            var userList = db.TaskCreatorUser.Where(x => x.TaskID == id).ToList();
            List<TaskHierarchy> emp = new List<TaskHierarchy>();
            bool pres = false;
            foreach (var ul in userList)
            {
                var name1 = db.UserProfile.FirstOrDefault(x => x.UserID == ul.CreatorID).FullName;
                var name = db.UserProfile.FirstOrDefault(x => x.UserID == ul.UserID).FullName;

                if (pres == false)
                {
                    if (ul.CreatorID != null)
                    {
                        string i = "" + ul.CreatorID;
                        TaskHierarchy e = new TaskHierarchy
                        {
                            ID = int.Parse(i),
                            Name = name1,
                            ManagerID = null
                        };
                        emp.Add(e);
                        TaskHierarchy e1 = new TaskHierarchy
                        {
                            ID = ul.UserID,
                            Name = name,
                            ManagerID = ul.CreatorID
                        };
                        emp.Add(e1);
                    }
                    else
                    {

                    }
                }
                else
                {

                    TaskHierarchy e = new TaskHierarchy
                    {
                        ID = ul.UserID,
                        Name = name,
                        ManagerID = ul.CreatorID
                    };
                    emp.Add(e);
                }
            }
            ViewBag.DocumentsOfTask = db.DocumentsCreated.Where(x => x.TaskID == id && x.UserID == null).OrderByDescending(x => x.CreatedOnDate).ToList();
            var president = emp.FirstOrDefault(x => x.ManagerID == null);
            SetChildren(president, emp);
            ViewBag.Users = president;
            return View(t);
        }

        private void SetChildren(TaskHierarchy model, List<TaskHierarchy> userList)
        {
            var children = userList.Where(x => x.ManagerID == model.ID).ToList();
            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    // SetChildren(model, userList);
                    model.Users.Add(child);
                }
            }
        }


        private List<UserProfile> returnUsersValid(int GroupId)
        {
            var getSuperAdmin = db.UserProfile.FirstOrDefault(x => x.Username == "engjellahmeti");
            List<UserProfile> listaEUserave = new List<UserProfile>();
            switch(GroupId)
            {
                case 1:
                    listaEUserave = db.UserProfile.Where(x => x.UserID != getSuperAdmin.UserID).ToList();
                    break;
                case 2:
                    listaEUserave = db.UserProfile.Where(x => x.UserID != getSuperAdmin.UserID && x.GroupID != 1).ToList();
                    break;
                case 3:
                    listaEUserave = db.UserProfile.Where(x => x.UserID != getSuperAdmin.UserID && x.GroupID != 1 && x.GroupID != 2).ToList();
                    break;
                case 4:
                    listaEUserave = db.UserProfile.Where(x => x.UserID != getSuperAdmin.UserID && x.GroupID != 1 && x.GroupID != 2 && x.GroupID != 3).ToList();
                    break;
                case 5:
                    listaEUserave = db.UserProfile.Where(x => x.UserID != getSuperAdmin.UserID && x.GroupID != 1 && x.GroupID != 2 && x.GroupID != 3 && x.GroupID != 4).ToList();
                    break;

            }
            return listaEUserave;
        }


    }
}
