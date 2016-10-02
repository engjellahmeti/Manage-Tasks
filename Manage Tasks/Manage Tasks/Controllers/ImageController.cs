using Manage_Tasks.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Manage_Tasks.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        dbManageTasks db = new dbManageTasks();



        public ActionResult RetrieveImage(string username)
        {
            var userprofile = db.UserProfile.FirstOrDefault(x => x.UsID.Contains(username));
            if (userprofile != null)
            {
                byte[] profilePic = getImageFromDatabase(userprofile.UserID);
                if (profilePic != null)
                {
                    return File(profilePic, "image/jpg");
                }
                else
                {
                    return null;
                }

            }

            return null;
        }
        public ActionResult RetrieveImageTask(int id)
        {
            byte[] profilePic = getImageFromDatabase(id);
            if (profilePic != null)
            {
                return File(profilePic, "image/jpg");
            }
            else
            {
                return null;
            }



            return null;
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            var id = Request["id"].ToString();
            //var UId = from temp in db.User
            //             where temp.UsID == id
            //             select temp.UserID;
            var UId = db.UserProfile.FirstOrDefault(x => x.UsID == id);
            Image img = new Image();
           // img.File = file;

            img.FileName = file.FileName;
            img.ImageSize = file.ContentLength;

            byte[] imageBytes = null;

            BinaryReader reader = new BinaryReader(file.InputStream);
            imageBytes = reader.ReadBytes((int)file.ContentLength);
            img.Image1 = imageBytes;
            String uid = "" + UId.UserID;
            int imgUserId = int.Parse(uid);
            img.ID_User = imgUserId;
            img.IsDeleted = false;
            img.CreatedOnDate = DateTime.Now;
            img.LastModifiedByUserID = id;
            img.LastModifiedOnDate = DateTime.Now;

            if (db.Image.Any(x => x.Image1 == imageBytes && x.ID_User == imgUserId))
            {
                db.Image.FirstOrDefault(x => x.Image1 == imageBytes && x.ID_User == imgUserId).IsDeleted = false;
                db.Image.FirstOrDefault(x => x.IsDeleted == false && x.ID_User == imgUserId).IsDeleted = true;
            }
            else
            {
                try
                {
                    db.Image.FirstOrDefault(x => x.IsDeleted == false && x.ID_User == imgUserId).IsDeleted = true;
                    db.Image.Add(img);
                }
                catch
                {
                    db.Image.Add(img);
                }
            }
            db.SaveChanges();


            return RedirectToAction("profileGiven", "MyProfile");
        }

       [HttpPost]
        public JsonResult UploadDocuments()
        {
            try
            {
                if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var pic = System.Web.HttpContext.Current.Request.Files["HelpSectionImages"];
                    HttpPostedFileBase file = new HttpPostedFileWrapper(pic);
                    var taskId = Request["taskId"].ToString();
                    var userId = Request["userId"].ToString();
                    int userid = int.Parse(userId);
                    int taskid = int.Parse(taskId);
                    var UId = db.UserProfile.FirstOrDefault(x => x.UserID == userid);
                    DocumentsCreated dc = new DocumentsCreated();
                    //dc.File = file;
                    dc.FileName = file.FileName;
                    dc.FileSize = file.ContentLength;

                    byte[] imageBytes = null;

                    BinaryReader reader = new BinaryReader(file.InputStream);
                    imageBytes = reader.ReadBytes((int)file.ContentLength);
                    dc.Dokument = imageBytes;
                    String uid = "" + UId.UserID;
                    int imgUserId = int.Parse(uid);
                    dc.UserID = imgUserId;
                    dc.TaskID = taskid;
                    dc.IsDeleted = false;
                    dc.CreatedOnDate = DateTime.Now;
                    dc.LastModifiedByUserID = UId.UsID;
                    dc.LastModifiedOnDate = DateTime.Now;
                    db.DocumentsCreated.Add(dc);
                    db.SaveChanges();

                }

            }
            catch(DataException ex)
            {
                throw ex;
            }


            return Json(null, JsonRequestBehavior.AllowGet);
         }

        public byte[] getImageFromDatabase(int id)
        {
            var imageBytes = from d in db.Image
                             where d.ID_User == id && d.IsDeleted == false
                             select d.Image1;

            byte[] profilePic = null;
            try
            {
                profilePic = imageBytes.First();
            }
            catch
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(HttpContext.Server.MapPath("~/Content/ProfilePicture.png"));
                profilePic = imageToByteArray(image);
            }

            return profilePic;
        }

        public byte[] imageToByteArray(System.Drawing.Image image)
        {
            MemoryStream m = new MemoryStream();
            image.Save(m, System.Drawing.Imaging.ImageFormat.Gif);
            return m.ToArray();
        }

        [HttpGet]
        public void deleteDocument(int id)
        {
            try
            {
                DocumentsCreated d1 = db.DocumentsCreated.Find(id);
                DocumentsCreated d2 = db.DocumentsCreated.Find(id);
                d2.IsDeleted = true;
                db.Entry(d1).CurrentValues.SetValues(d2);
                db.Entry(d1).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (DataException ex)
            {
                throw ex;
            }
        }


        public ActionResult downloadFile(int id)
        {
            var docs = db.DocumentsCreated.FirstOrDefault(x => x.ID == id);

            return File(docs.Dokument, System.Net.Mime.MediaTypeNames.Application.Octet, docs.FileName);
        }
    }
}