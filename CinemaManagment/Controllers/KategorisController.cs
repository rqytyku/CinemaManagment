using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CinemaManagment.Models;
using CinemaManagment.Models;
using NLog;
namespace CinemaManagment.Controllers
{
    public class KategorisController : Controller
    {
        private KinemaDBEntities db = new KinemaDBEntities();
        Logger logger = LogManager.GetCurrentClassLogger();

        // GET: Kategoris
        public ActionResult Index()
        {
            try
            {
                return View(db.Kategoris.ToList());
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend Ne Kategori Controller,Index  Action", ex);
            }
            return View();
        }

        // GET: Kategoris/Create - Krijimi i nje Kategorie te re
        public ActionResult Create()
        {
            if (Session["menaxher"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Kategoris");
            }
        }

        // POST: Kategoris/Create
        [HttpPost]
        public ActionResult Create(Kategori K)
        {
            try
            {
                string filename = Path.GetFileNameWithoutExtension(K.imagefile.FileName);
                string extenction = Path.GetExtension(K.imagefile.FileName);
                filename = filename + DateTime.Now.ToString("yymmssfff") + extenction;
                K.Foto = "~/fonts/Img/" + filename;
                K.imagefile.SaveAs(Server.MapPath("~/fonts/Img/" + filename));
                filename = Path.Combine(Server.MapPath("~/fonts/Img/") + filename);

                db.Kategoris.Add(K);
                db.SaveChanges();
                TempData["rezsuccess"] = "<script>alert('U shtua me sukses!');</script>";

                ModelState.Clear();
                return View();
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Kontroller Kategori, Create Action", ex);
            }
            return View();
        }

        // GET: Kategoris/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["menaxher"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Kategori kategori = db.Kategoris.Find(id);
                if (kategori == null)
                {
                    return HttpNotFound();
                }
                return View(kategori);
            }
            else
            {
                return RedirectToAction("Index", "Kategoris");
            }
        }

        // POST: Kategoris/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Kategori kategori = db.Kategoris.Find(id);
                db.Kategoris.Remove(kategori);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                logger.ErrorException("Error u gjend ne Kategori Controller, Delete Action", ex);
            }
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
