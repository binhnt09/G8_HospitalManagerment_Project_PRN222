using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.LaboratoryController
{
    public class TestsController : Controller
    {
        private readonly ItestService _service;

        public TestsController(ItestService service)
        {
            _service = service;
        }

        // GET: Tests
        public async Task<IActionResult> Index(string search, string sort, int? pageNumber)
        {
            int pageSize = 6;
            int pageIndex = pageNumber ?? 1;

            ViewBag.CurrentSort = sort;
            ViewBag.CostSortParm = String.IsNullOrEmpty(sort) ? "cost_desc" : "";
            ViewBag.CurrentFilter = search;
            ViewBag.CurrentPage = pageIndex;

            var dataResult = await _service.GetIndexDataAsync(search, sort, pageIndex, pageSize);

            ViewBag.TotalOrders = dataResult.TotalOrders;

            ViewBag.TotalPages = dataResult.TotalPages;
            ViewBag.TotalItems = dataResult.TotalOrders;
            ViewBag.ItemStart = dataResult.ItemStart;
            ViewBag.ItemEnd = dataResult.ItemEnd;

            return View(dataResult.TestPagedData);
        }

        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _service.GetTestDetailsAsync(id.Value);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: Tests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TestId,TestName,Category,ReferenceRange,Cost,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Test test)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateTestAsync(test);
                return RedirectToAction(nameof(Index));
            }
            return View(test);
        }

        // GET: Tests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _service.GetTestDetailsAsync(id.Value);
            if (test == null)
            {
                return NotFound();
            }
            return View(test);
        }

        // POST: Tests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TestId,TestName,Category,ReferenceRange,Cost,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Test test)
        {
            if (id != test.TestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateTestAsync(test);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(test.TestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(test);
        }

        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _service.GetTestDetailsAsync(id.Value);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteTestAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool TestExists(int id)
        {
            return _service.CheckTestExists(id);
        }
    }
}
