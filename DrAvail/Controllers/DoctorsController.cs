using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DrAvail.Data;
using DrAvail.Models;

namespace DrAvail.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public Doctor doctor { get; set; }

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Doctors.Include(d => d.CommonAvailability).Include(d => d.CurrentAvailability).Include(d => d.Hospital);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.CommonAvailability)
                .Include(d => d.CurrentAvailability)
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        [HttpGet]
        public IActionResult Create()
        {
            #region selectList
            ViewData["CommonAvaliabilityID"] = new SelectList(_context.Availabilities, "ID", "ID");
            ViewData["CurrentAvaliabilityID"] = new SelectList(_context.Availabilities, "ID", "ID");
            ViewData["HospitalID"] = new SelectList(_context.Hospitals, "ID", "Name");

            var values = from HospitalType H in Enum.GetValues(typeof(HospitalType))
                         select new { ID = (int)H, Name = H.ToString() };
            ViewBag.HospitalType = new SelectList(values, "ID", "Name"); ;

            var district = from District d in Enum.GetValues(typeof(District))
                           select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Districts = new SelectList(district, "ID", "Name");

            var speciality = from Speciality d in Enum.GetValues(typeof(Speciality))
                             select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Speciality = new SelectList(speciality, "Name", "Name");

            var gender = from Gender H in Enum.GetValues(typeof(Gender))
                         select new { Name = H.ToString() };
            ViewBag.Gender = gender.ToList();
            #endregion

            return View();
        }

        // POST: Doctors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOnPost()
        {

            try
            {
                doctor.CommonAvailability.CommonDays.MorningStartTime = new DateTime(year: 2021, month: 3, day: 24,
                    hour: int.Parse(doctor.CommonAvailability.CommonDays.MorningStartHour),
                    minute: int.Parse(doctor.CommonAvailability.CommonDays.MorningStartMinute),
                    second: 0);
                doctor.CommonAvailability.AvailabilityType = doctor.RegNumber + "Common";
                //Console.WriteLine("----------------------------------------------");
                //Console.WriteLine("Before INSERT -> Hospital Id: " + doctor.HospitalID);
                //Console.WriteLine(doctor);
                //Console.WriteLine("----------------------------------------------");

                if (doctor.HospitalID != 0)
                {
                    doctor.Hospital = null;
                    //_context.Entry(doctor.Hospital).State = EntityState.Unchanged;
                }
                _context.Doctors.Add(doctor);

                await _context.SaveChangesAsync();
                //Console.WriteLine("----------------------------------------------");
                //Console.WriteLine("After INSERT -> doctor Id: " + doctor.ID);
                //Console.WriteLine(doctor);

                return RedirectToAction(nameof(Index));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            #region selectList
            ViewData["CommonAvaliabilityID"] = new SelectList(_context.Availabilities, "ID", "ID");
            ViewData["CurrentAvaliabilityID"] = new SelectList(_context.Availabilities, "ID", "ID");
            ViewData["HospitalID"] = new SelectList(_context.Hospitals, "ID", "Name");

            var values = from HospitalType H in Enum.GetValues(typeof(HospitalType))
                         select new { ID = (int)H, Name = H.ToString() };
            ViewBag.HospitalType = new SelectList(values, "ID", "Name"); ;

            var district = from District d in Enum.GetValues(typeof(District))
                           select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Districts = new SelectList(district, "ID", "Name");

            var speciality = from Speciality d in Enum.GetValues(typeof(Speciality))
                             select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Speciality = new SelectList(speciality, "ID", "Name");
            //var gender = from Gender H in Enum.GetValues(typeof(Gender))
            //             select new { Name = H.ToString() };
            //ViewBag.Gender = gender.ToList();
            ViewBag.Gender = Enum.GetNames(typeof(Gender)).Cast<Gender>().ToList();
            #endregion

            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            ViewData["CommonAvaliabilityID"] = new SelectList(_context.Availabilities, "ID", "ID", doctor.CommonAvaliabilityID);
            ViewData["CurrentAvaliabilityID"] = new SelectList(_context.Availabilities, "ID", "ID", doctor.CurrentAvaliabilityID);
            ViewData["HospitalID"] = new SelectList(_context.Hospitals, "ID", "Name", doctor.HospitalID);
            var district = from District d in Enum.GetValues(typeof(District))
                           select new { ID = (int)d, Name = d.ToString() };
            //ViewBag.Districts = new SelectList(district, "ID", "Name");

            var speciality = from Speciality d in Enum.GetValues(typeof(Speciality))
                             select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Speciality = new SelectList(speciality, "Name", "Name");
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,RegNumber,Speciality,Degree,Age,Gender,Practice,Experience,IsVerified,Summary,City,District,EmailId,PhoneNumber,HospitalID,CommonAvaliabilityID,CurrentAvaliabilityID")] Doctor doctor)
        {
            if (id != doctor.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.ID))
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
            ViewData["CommonAvaliabilityID"] = new SelectList(_context.Availabilities, "ID", "ID", doctor.CommonAvaliabilityID);
            ViewData["CurrentAvaliabilityID"] = new SelectList(_context.Availabilities, "ID", "ID", doctor.CurrentAvaliabilityID);
            ViewData["HospitalID"] = new SelectList(_context.Hospitals, "ID", "Name", doctor.HospitalID);
            var district = from District d in Enum.GetValues(typeof(District))
                           select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Districts = new SelectList(district, "ID", "Name");

            var speciality = from Speciality d in Enum.GetValues(typeof(Speciality))
                             select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Speciality = new SelectList(speciality, "Name", "Name");
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.CommonAvailability)
                .Include(d => d.CurrentAvailability)
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.ID == id);
        }

        public IActionResult AvailabilityCreate()
        {
            return View();
        }

        public JsonResult GetLocations(int Pincode)
        {
            var locations = _context.Locations
                .FromSqlRaw("SELECT * FROM Locations WHERE Pincode ==@Pincode", Pincode);
            Console.WriteLine("Pincode: " + Pincode);
            var locations2 = from location in _context.Locations
                             where location.Pincode == Pincode
                             select new { Locality = location.Locality, Dis = location.District };
            
            return Json(locations2.ToList());
        }

    }

}