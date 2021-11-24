using CSD_3354_Project_DataAccess.Data;
using CSD_3354_Project_DataAccess.Repository.IRepository;
using CSD_3354_Project_Models;
using CSD_3354_Project_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//Password for all users: Temp1234*
namespace CSD_3354_Project.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {
        //private readonly ApplicationDbContext _db;
        private readonly ICategoryRepository _catRepo;
        //public CategoryController(ApplicationDbContext db)
        public CategoryController(ICategoryRepository catRepo)
        {
            //_db = db;
            _catRepo = catRepo;
        }
        public IActionResult Index()
        {
            //IEnumerable<Category> objList = _db.Category;
            IEnumerable<Category> objList = _catRepo.GetAll();
            return View(objList);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                //_db.Category.Add(obj);
                //_db.SaveChanges();
                _catRepo.Add(obj);
                _catRepo.Save();
                TempData[WC.Success] = "Category created successfully";
                return RedirectToAction("Index");
            }
            TempData[WC.Error] = "Error while creating category";
            return View(obj);

        }

        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //var obj = _db.Category.Find(id);
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                //_db.Category.Update(obj);
                //_db.SaveChanges();
                _catRepo.Update(obj);
                _catRepo.Save();
                TempData[WC.Success] = "Action completed successfully";
                return RedirectToAction("Index");
            }
            return View(obj);

        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //var obj = _db.Category.Find(id);
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            //var obj = _db.Category.Find(id);
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            //_db.Category.Remove(obj);
            //_db.SaveChanges();
            TempData[WC.Success] = "Action completed successfully";
            _catRepo.Remove(obj);
            _catRepo.Save();
            return RedirectToAction("Index");


        }
    }
}
