using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MovieDB.Models;
using Microsoft.AspNet.Identity;

namespace MovieDB.Controllers
{
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Comments
        public ActionResult Index()
        {
            var comments = db.Comments.Include(c => c.Movie);
            return View(comments.ToList());
        }

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        [Authorize]
        public ActionResult Create(int movieId)
        {
            var newComment = new Comment();
            newComment.MovieId = movieId;

            return View(newComment);
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CommentId,Text,MovieId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                var currentid = comment.MovieId;
                db.Comments.Add(comment);
                db.SaveChanges();
                ControllerContext.HttpContext.Response.Redirect(ControllerContext.HttpContext.Request.Url.ToString());
            }

            return View();
        }

        // GET: Comments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            ViewBag.MovieId = new SelectList(db.Movies, "Id", "Title", comment.MovieId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CommentId,Text,Date,Author_Id,MovieId")] Comment comment)

        //public ActionResult Edit([Bind(Include = "Text")] Comment comment)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            if (ModelState.IsValid)
            {
                comment.Date = DateTime.Now;
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                var movieId = comment.MovieId;
                string redirectLink = "/Movies/Details/" + movieId.ToString();
                return Redirect(redirectLink);
            }
            ViewBag.MovieId = new SelectList(db.Movies, "Id", "Title", comment.MovieId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
            db.SaveChanges();
            var movieId = comment.MovieId;
            string redirectLink = "/Movies/Details/" + movieId.ToString();
            return Redirect(redirectLink);
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
