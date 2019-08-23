using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
 
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            IndexViewModel viewModel = new IndexViewModel(){
                User = new User(),
                Login = new Login()
            };
            return View(viewModel);
        }

        [HttpPost("register")]
        public IActionResult Register(IndexViewModel modelData) {
            System.Console.WriteLine("************* in Register");
            User user = modelData.User;
            if(ModelState.IsValid) {
                System.Console.WriteLine("********************* ModelState.IsValid");


                // Email is the first example of a way you can have a specific condition to error out
                // The condition of what you compare in your database lookup (u=> u.Property) will changed based on what
                // you want to compare in the models
                if(dbContext.Users.Any(u => u.Email == user.Email)) {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }

                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                HttpContext.Session.SetString("Name", userInDb.FirstName);
                HttpContext.Session.SetInt32("Id", userInDb.id);
                return RedirectToAction("Dashboard");
            }
            System.Console.WriteLine("******************* ModelState.Is NOT Valid");
            return View("Index");
        }

        [HttpPost("login")]
        public IActionResult Login(IndexViewModel modelData) {
            System.Console.WriteLine("************* in Login");
            Login user = modelData.Login;
            System.Console.WriteLine("##################### user.Email: " + user.Email + " user.Password " + user.Password);
            if(ModelState.IsValid) {
                System.Console.WriteLine("********************* ModelState.IsValid");
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                if(userInDb == null) {
                    System.Console.WriteLine("********************* userInDb == null");
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<Login>();
                var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.Password);
                if(result == 0) {
                    System.Console.WriteLine("%%%%%%%%%%%%%%%%%%% Password Matches off to the Dashboard");
                    return View("Index");
                }
                HttpContext.Session.SetString("Name", userInDb.FirstName);
                ViewBag.Name = HttpContext.Session.GetString("Name");
                HttpContext.Session.SetInt32("Id", userInDb.id);
                ViewBag.Id = HttpContext.Session.GetInt32("Id");
                return RedirectToAction("Dashboard");
            }
            System.Console.WriteLine("******************* ModelState.Is NOT Valid");
            return View("Index");
        }

        [HttpPost("create")]
        public IActionResult Create(Wedding modelData) {

            // getting id that is in session and going into the current database and setting it to a variable for use in the remainder of the function. 
            User user = dbContext.Users.FirstOrDefault(u => u.id == HttpContext.Session.GetInt32("Id"));
            if (ModelState.IsValid){
                if (modelData.weddingDate < DateTime.Today){
                    ModelState.AddModelError("Date", "Date Must Be in the Future Bitch! Dont get so excited!");
                    return View("New");
                }
                modelData.id = user.id;
                dbContext.Weddings.Add(modelData);
                // When adding to the database through your context, AFTER "dbcontext.SaveChanges();" the database details are RETURNED and inherited by the form that was passed in
                dbContext.SaveChanges();
                user.createdWedding.Add(modelData);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet("logout")]
         public IActionResult Logout() {
             HttpContext.Session.Clear();
             return RedirectToAction("Index");
         }
        // public IActionResult Privacy()
        // {
        //     return View();
        // }
        [HttpGet("details/{id}")]
        public IActionResult Details (int id){
            int? userid = HttpContext.Session.GetInt32("Id");
            if (userid == null){
                // redirectToAction redirects to a function 
                return RedirectToAction("Index");
            }
            // OOP: Create an instance of my class of "Wedding" named 'wedding'
            Wedding wedding = dbContext.Weddings.Include(w => w.creator).Include(w => w.Rsvp).ThenInclude(r => r.user).FirstOrDefault(w => w.weddingId == id);
            return View(wedding);
        }
        // [HttpPost("rsvp")]
        public IActionResult Rsvp (int id, Rsvp modelData){
            // Checks if user is in session and gets it 
            int? userid = HttpContext.Session.GetInt32("Id");
            if (userid == null){
                return RedirectToAction("Index");
            }

            // Checks if wedding is in database and gets it
            Wedding wedding = dbContext.Weddings.Include(w=> w.Rsvp).FirstOrDefault(w=> w.weddingId == id);

            if(wedding == null)
            {
                return RedirectToAction("Dashboard");
            }
            ///////////////////////////////////
            User user = dbContext.Users.Include(u => u.Rsvp).FirstOrDefault(u => u.id == userid);


            Rsvp rsvp = dbContext.Rsvps.FirstOrDefault(r => r.UserId == user.id && r.WeddingId == id);
            if(rsvp == null)
            {
                modelData.UserId = user.id;
                modelData.WeddingId = id;
                dbContext.Rsvps.Add(modelData);
                dbContext.SaveChanges();
                user.Rsvp.Add(modelData);
                wedding.Rsvp.Add(modelData);
                dbContext.SaveChanges();
            }
            else if(rsvp != null)
            {
                dbContext.Rsvps.Remove(rsvp);
                dbContext.SaveChanges();
                user.Rsvp.Remove(rsvp);
                wedding.Rsvp.Remove(rsvp);
                dbContext.SaveChanges();
            }

            return RedirectToAction("Dashboard");
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            // PLACE ALL the code below at the top of every method route that renders a view that you dont want anyone to access it who's not in session. *******
            // use '?' after 'int' to let it be an 'int' or 'null'
            int? userid = HttpContext.Session.GetInt32("Id");
            if (userid == null){
                // redirectToAction redirects to a function 
                return RedirectToAction("Index");
            }

            User user = dbContext.Users.Include(u => u.Rsvp).FirstOrDefault(u => u.id == userid);

            List<Wedding> allWeddings = dbContext.Weddings.Include(w => w.creator).Include(w=> w.Rsvp).ThenInclude(r=> r.user).ToList();

            List<Wedding> reservedWeddings = dbContext.Rsvps.Include(r=> r.wedding).Where(r=> r.UserId == user.id).Select(r => r.wedding).ToList();

            ViewBag.User = user;
            ViewBag.AllWeddings = allWeddings;
            ViewBag.ReservedWeddings = reservedWeddings;
            return View();
        }
        [HttpGet("create")]
        public IActionResult New()
        {
            // PLACE ALL the code below at the top of every method route that renders a view that you dont want anyone to access it who's not in session. *******
            // use '?' after 'int' to let it be an 'int' or 'null'
            int? userid = HttpContext.Session.GetInt32("Id");
            if (userid == null){
                // redirectToAction redirects to a function 
                return RedirectToAction("Index");
            }

            return View("New");
        }

        [HttpGet("details/{id}/delete")]
        public IActionResult Delete (int id) {
            int? userid = HttpContext.Session.GetInt32("Id");
            if (userid == null){
                // redirectToAction redirects to a function 
                return RedirectToAction("Index");
            }
            // setting the user to a variable from session
            User user = dbContext.Users.Include(u => u.Rsvp).FirstOrDefault(u => u.id == userid);

            // This gets a wedding based on our route id
            Wedding wedding = dbContext.Weddings.Include(w => w.creator).Include(w => w.Rsvp).ThenInclude(r => r.user).FirstOrDefault(w => w.weddingId == id);

            if (wedding.creator == user){
                user.createdWedding.Remove(wedding);
                dbContext.Weddings.Remove(wedding);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
