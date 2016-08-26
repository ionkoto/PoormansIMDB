﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MovieDB.Models;
using System.Security;
using Microsoft.AspNet.Identity;

namespace MovieDB.Controllers
{
    [ValidateInput(false)]
    public class MoviesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Movies
        public ActionResult Index()
        {
            var moviesWithAuthors = db.Movies.Include(m => m.Author).ToList();
            return View(moviesWithAuthors);
        }

        // GET: Movies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // GET: Movies/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }
        
        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Body")] Movie movie, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var streamLength = image.InputStream.Length;
                var imageBytes = new byte[streamLength];
                image.InputStream.Read(imageBytes, 0, imageBytes.Length);
                movie.Image = imageBytes;
                movie.Author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                db.Movies.Add(movie);
              
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            Movie movie = db.Movies.Find(id);
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

            if(movie.Author_Id != user.Id && !User.IsInRole("Administrators"))
            {
                throw new SecurityException("Unauthorized access!");
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (movie == null)
                {
                    return HttpNotFound();
                }
                var authors = db.Users.ToList();
                ViewBag.Authors = authors;
                return View(movie);
            }
          
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]

        public ActionResult Edit([Bind(Include = "Id,Title,Body,Date,Author_Id")] Movie movie, HttpPostedFileBase image)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            
            if (ModelState.IsValid)
            {
                if (!User.IsInRole("Administrators"))
                {
                    movie.Author_Id = user.Id;
                }
                if (image != null)
                {
                    var streamLength = image.InputStream.Length;
                    var imageBytes = new byte[streamLength];
                    image.InputStream.Read(imageBytes, 0, imageBytes.Length);
                    movie.Image = imageBytes;
                }
                db.Entry(movie).State = EntityState.Modified;
                if (image == null)
                {
                    db.Entry(movie).Property(x => x.Image).IsModified = false;
                }
                db.SaveChanges();
                return RedirectToAction("Index");     
            }
            return View(movie);
            
        }

        // GET: Movies/Delete/5
        [Authorize]

        public ActionResult Delete(int? id)
        {
            Movie movie = db.Movies.Find(id);
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

            if (movie.Author_Id != user.Id && !User.IsInRole("Administrators"))
            {
                throw new SecurityException("Unauthorized access!");
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (movie == null)
                {
                    return HttpNotFound();
                }
                return View(movie);
            }
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]

        public ActionResult DeleteConfirmed(int id)
        {
            Movie movie = db.Movies.Find(id);
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

            if (movie.Author_Id != user.Id && !User.IsInRole("Administrators"))
            {
                throw new SecurityException("Unauthorized access!");
            }
            else
            {
                db.Movies.Remove(movie);
                db.SaveChanges();
                return RedirectToAction("Index");
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
    }
}
