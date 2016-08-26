using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MovieDB.Models;


namespace MovieDB.Controllers
{
    [ValidateInput(false)]
    public class MoviesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Movies
        //public ActionResult Index()
        //{
        //    var moviesWithAuthors = db.Movies.Include(m => m.Author).ToList();
        //    return View(moviesWithAuthors);
        //}

        public ActionResult Index(string sortOrder, string searchTitle, string searchBody)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.AuthorSortParm = sortOrder == "Author" ? "author_desc" : "Author";



            var movies = from m in db.Movies.Include(mov => mov.Author).ToList()
                           select m;

            if (!String.IsNullOrEmpty(searchTitle))
            {
                movies = movies.Where(m => m.Title.IndexOf(searchTitle,StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (!String.IsNullOrEmpty(searchBody))
            {
                movies = movies.Where(m => m.Body.IndexOf(searchBody, StringComparison.OrdinalIgnoreCase) >= 0);
            }


            switch (sortOrder)
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
            return View(movies.ToList());
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
        public ActionResult Create([Bind(Include = "Id,Title,Body")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                movie.Author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                db.Movies.Add(movie);
              
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        [Authorize(Roles = "Administrators")]
        public ActionResult Edit(int? id)
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
            var authors = db.Users.ToList();
            ViewBag.Authors = authors;
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]

        public ActionResult Edit([Bind(Include = "Id,Title,Body,Date, Author_Id")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        [Authorize(Roles = "Administrators")]

        public ActionResult Delete(int? id)
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

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]

        public ActionResult DeleteConfirmed(int id)
        {
            Movie movie = db.Movies.Find(id);
            db.Movies.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
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
