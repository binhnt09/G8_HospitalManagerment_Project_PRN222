using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.BillingController
{
    public class InvoicesController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public InvoicesController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // GET: Invoices
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            // 1. Preserve search/sort state for the view
            ViewBag.CurrentSort  = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.CurrentFilter = searchString;

            // 2. Base query with all required includes
            var invoices = _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Appointment)
                .AsQueryable();

            // 3. Summary cards — computed on the FULL data set (before filtering)
            ViewBag.TotalInvoices  = await invoices.CountAsync();
            ViewBag.PaidCount      = await invoices.CountAsync(i => i.Status == "Paid");
            ViewBag.UnpaidCount    = await invoices.CountAsync(i => i.Status == "Unpaid");
            ViewBag.TotalRevenue   = await invoices
                                        .Where(i => i.Status == "Paid")
                                        .SumAsync(i => (decimal?)i.FinalAmount) ?? 0m;

            // 4. Search — matches on Patient name
            if (!string.IsNullOrEmpty(searchString))
            {
                var search = searchString.ToLower();
                invoices = invoices.Where(i =>
                    i.Patient != null && i.Patient.User != null &&
                    (i.Patient.User.FirstName + " " + i.Patient.User.LastName)
                        .ToLower().Contains(search));
            }

            // 5. Sort
            invoices = sortOrder switch
            {
                "date_desc"    => invoices.OrderByDescending(i => i.IssueDate),
                "amount_asc"   => invoices.OrderBy(i => i.FinalAmount),
                "amount_desc"  => invoices.OrderByDescending(i => i.FinalAmount),
                "status_asc"   => invoices.OrderBy(i => i.Status),
                _              => invoices.OrderBy(i => i.IssueDate)   // default: oldest first
            };

            // 6. Pagination
            const int pageSize = 6;
            int pageIndex  = pageNumber ?? 1;
            int totalItems = await invoices.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.CurrentPage = pageIndex;
            ViewBag.TotalPages  = totalPages;
            ViewBag.TotalItems  = totalItems;
            ViewBag.ItemStart   = totalItems == 0 ? 0 : (pageIndex - 1) * pageSize + 1;
            ViewBag.ItemEnd     = Math.Min(pageIndex * pageSize, totalItems);

            var pagedData = await invoices
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(pagedData);
        }


        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Appointment)
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Invoices/Create
        public IActionResult Create()
        {
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId");
            return View();
        }

        // POST: Invoices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceId,PatientId,AppointmentId,TotalAmount,Discount,FinalAmount,Status,IssueDate,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invoice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", invoice.AppointmentId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", invoice.PatientId);
            return View(invoice);
        }

        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            // Business rule: only Unpaid invoices can be edited
            if (invoice.Status == "Paid" || invoice.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Cannot edit an invoice that is already Paid or Cancelled!";
                return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
            }

            return View(invoice);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InvoiceId,Discount,Status")] Invoice invoice)
        {
            // Step 1: Route/body id mismatch guard
            if (id != invoice.InvoiceId)
                return NotFound();

            // Step 2: Fetch the real record from DB (with Patient.User for view fallback)
            var existingInvoice = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);

            if (existingInvoice == null)
                return NotFound();

            // Step 4: Security check — cannot modify a locked invoice
            if (existingInvoice.Status == "Paid" || existingInvoice.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Cannot edit an invoice that is already Paid or Cancelled!";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Step 5: Apply only the two allowed fields
            existingInvoice.Discount  = invoice.Discount;
            existingInvoice.Status    = invoice.Status;

            // Step 6: Recalculate FinalAmount server-side
            existingInvoice.FinalAmount = existingInvoice.TotalAmount - (existingInvoice.Discount ?? 0m);
            existingInvoice.UpdatedAt   = DateTime.Now;

            // Step 7: Save
            try
            {
                _context.Update(existingInvoice);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Invoice updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(existingInvoice.InvoiceId))
                    return NotFound();
                throw;
            }
        }


        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Appointment)
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET: Invoices/GenerateInvoice/5
        // Auto-generates an Invoice (with line items) for a completed Appointment.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> GenerateInvoice(int appointmentId)
        {
            // ── Step 1a: Fetch the Appointment with all billing-related data ──────
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.MedicalRecords)
                    .ThenInclude(mr => mr.LabOrders)
                        .ThenInclude(lo => lo.LabOrderItems)
                            .ThenInclude(loi => loi.Test)
                .Include(a => a.MedicalRecords)
                    .ThenInclude(mr => mr.Prescriptions)
                        .ThenInclude(p => p.PrescriptionItems)
                            .ThenInclude(pi => pi.Drug)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound();

            // ── Step 1b: Guard – redirect if an invoice already exists ────────────
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

            if (existingInvoice != null)
            {
                TempData["InfoMessage"] = "An invoice for this appointment already exists.";
                return RedirectToAction(nameof(Details), new { id = existingInvoice.InvoiceId });
            }

            // ── Step 2 + 3: Build the Invoice and its line items inside a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoiceItems = new List<InvoiceItem>();

                // ── 3a: Consultation Fee ──────────────────────────────────────────
                invoiceItems.AddRange(await BuildConsultationItemAsync(appointment));
                
                // ── 3b: Lab-Test Fees ─────────────────────────────────────────────
                invoiceItems.AddRange(BuildLabTestItems(appointment));

                // ── 3c: Pharmacy Fees ─────────────────────────────────────────────
                invoiceItems.AddRange(BuildPharmacyItems(appointment));

                // ── Step 4: Totals ────────────────────────────────────────────────
                decimal discount = 0m;
                decimal totalAmount = invoiceItems.Sum(i => i.TotalPrice);
                decimal finalAmount = totalAmount - discount;

                var invoice = new Invoice
                {
                    PatientId     = appointment.PatientId,
                    AppointmentId = appointment.AppointmentId,
                    Status        = "Unpaid",
                    IssueDate     = DateTime.Now,
                    CreatedAt     = DateTime.Now,
                    Discount      = discount,
                    TotalAmount   = totalAmount,
                    FinalAmount   = finalAmount
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync(); // invoice.InvoiceId is now populated

                // Attach the InvoiceId to every line item before inserting
                foreach (var item in invoiceItems)
                    item.InvoiceId = invoice.InvoiceId;

                _context.InvoiceItems.AddRange(invoiceItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // ── Step 5: Redirect to the new invoice's Details page ────────────
                TempData["SuccessMessage"] = "Invoice generated successfully!";
                return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // re-throw so the developer error page / global handler catches it
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helper – Consultation Fee line item
        // Looks up the "General Consultation" Service from the database.
        // Falls back to a hard-coded fee of 150,000 VND if the service is missing.
        // ─────────────────────────────────────────────────────────────────────
        private async Task<List<InvoiceItem>> BuildConsultationItemAsync(Appointment appointment)
        {
            const decimal FallbackConsultationFee = 150_000m;

            var consultationService = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceName == "General Consultation");

            decimal unitPrice = consultationService?.Price ?? FallbackConsultationFee;
            int serviceRefId  = consultationService?.ServiceId ?? 0;

            return new List<InvoiceItem>
            {
                new InvoiceItem
                {
                    ItemType   = "Service",
                    ReferenceId = serviceRefId,
                    ItemName   = "General Consultation",
                    Quantity   = 1,
                    UnitPrice  = unitPrice,
                    TotalPrice = unitPrice,
                    CreatedAt  = DateTime.Now
                }
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helper – Lab-Test line items
        // One InvoiceItem per LabOrderItem across all MedicalRecords.
        // ─────────────────────────────────────────────────────────────────────
        private static List<InvoiceItem> BuildLabTestItems(Appointment appointment)
        {
            var items = new List<InvoiceItem>();

            foreach (var medRecord in appointment.MedicalRecords)
            {
                foreach (var labOrder in medRecord.LabOrders)
                {
                    foreach (var labOrderItem in labOrder.LabOrderItems)
                    {
                        decimal cost = labOrderItem.Test?.Cost ?? 0m;

                        items.Add(new InvoiceItem
                        {
                            ItemType    = "LabTest",
                            ReferenceId = labOrderItem.OrderItemId,
                            ItemName    = labOrderItem.Test?.TestName ?? "Lab Test",
                            Quantity    = 1,
                            UnitPrice   = cost,
                            TotalPrice  = cost,
                            CreatedAt   = DateTime.Now
                        });
                    }
                }
            }

            return items;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helper – Pharmacy / Drug line items
        // One InvoiceItem per PrescriptionItem across all Prescriptions.
        // PrescriptionItem.Quantity already exists in the model.
        // ─────────────────────────────────────────────────────────────────────
        private static List<InvoiceItem> BuildPharmacyItems(Appointment appointment)
        {
            var items = new List<InvoiceItem>();

            foreach (var medRecord in appointment.MedicalRecords)
            {
                foreach (var prescription in medRecord.Prescriptions)
                {
                    foreach (var prescriptionItem in prescription.PrescriptionItems)
                    {
                        decimal unitPrice = prescriptionItem.Drug?.Price ?? 0m;
                        int quantity      = prescriptionItem.Quantity ?? 1; // default to 1 if null

                        items.Add(new InvoiceItem
                        {
                            ItemType    = "Pharmacy",
                            ReferenceId = prescriptionItem.PrescriptionItemId,
                            ItemName    = prescriptionItem.Drug?.DrugName ?? "Drug",
                            Quantity    = quantity,
                            UnitPrice   = unitPrice,
                            TotalPrice  = unitPrice * quantity,
                            CreatedAt   = DateTime.Now
                        });
                    }
                }
            }

            return items;
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }
    }
}
