using System;
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
using PagedList;

namespace MovieDB.Controllers
{
    [ValidateInput(false)]
    public class MoviesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        public ActionResult Index(string option, string search, int? pageNumber, string sort)
        {
            
            ViewBag.NameSortParm = String.IsNullOrEmpty(sort) ? "name_desc" : "";
            ViewBag.DateSortParm = sort == "Date" ? "date_desc" : "Date";
            ViewBag.AuthorSortParm = sort == "Author" ? "author_desc" : "Author";

            var movies = from m in db.Movies.Include(mov => mov.Author).ToList()
                         select m;
            if (option == "Title" && !String.IsNullOrEmpty(search))
            {
                movies = movies.Where(m => m.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (option== "Description Keyword" && !String.IsNullOrEmpty(search)) 
            {
                movies = movies.Where(m => m.Body.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (option == "Author" && !String.IsNullOrEmpty(search))
            {
                movies = movies.Where(m => m.Author.Email.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            switch (sort)
            {
                case "name_desc":
                    movies = movies.OrderByDescending(m => m.Title);
                    break;
                case "Date":
                    movies = movies.OrderBy(m => m.Date);
                    break;
                case "date_desc":
                    movies = movies.OrderByDescending(m => m.Date);
                    break;

                case "Author":
                    movies = movies.OrderBy(m => m.Author.Email.ToString());
                    break;
                case "author_desc":
                    movies = movies.OrderByDescending(m => m.Author.Email.ToString());
                    break;

                default:
                    movies = movies.OrderBy(m => m.Title);
                    break;
            }
            
            return View(movies.ToList().ToPagedList(pageNumber ?? 1, 6));

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
