﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DrAvail.Data;
using DrAvail.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using DrAvail.Authorization;
using DrAvail.Services;
using Microsoft.Extensions.Logging;

namespace DrAvail.Controllers
{
    public class DoctorsController : DI_BaseController
    {
        //private readonly ApplicationDbContext _context;
        private readonly ILogger<DoctorsController> _logger;

        private readonly IDoctorService DoctorService;

        [BindProperty]
        public Doctor Doctor { get; set; }

        //public DrAvail.Services.PaginatedList<Doctor> Doctors { get; set; }

        public DoctorsController(IDoctorService doctorService, ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager,
            ILogger<DoctorsController> logger)
            : base(context, authorizationService, userManager)
        {
            DoctorService = doctorService;
            //Context = context;
            _logger = logger;
        }


        // GET: Doctors
        [AllowAnonymous]
        [RequireHttps]
        public async Task<IActionResult> Index(string currentFilter, string searchString, int? pageIndex)
        {
            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            IQueryable<Doctor> doctorsIQ = Context.Doctors
                .Include(d => d.CommonAvailability)
                .Include(d => d.CurrentAvailability)
                .Include(d => d.Hospital)
                .OrderBy(d => d.Name);

            if (!string.IsNullOrEmpty(searchString))
            {
                doctorsIQ = doctorsIQ.Where(d => d.Name.Contains(searchString));
            }

            //checks if a user is logged in
            if (User.Identity.Name != null)
            {
                var isAuthorized = User.IsInRole(Constants.AdministratorsRole);

                var currentUserId = UserManager.GetUserId(User);

                // Only verified doctors are shown UNLESS you're authorized to see them
                // or you are the owner.
                if (!isAuthorized)
                {
                    doctorsIQ = doctorsIQ.Where(d => d.IsVerified == true || d.OwnerID == currentUserId);
                }
            }
            else
            {
                doctorsIQ = doctorsIQ.Where(d => d.IsVerified == true);
            }
            //var pageSize = _configuration.GetValue("PageSize", 4); //Sets pageSize to 3 from Configuration, 4 if configuration fails.
            var pageSize = 4;
            return View(await DrAvail.Services.PaginatedList<Doctor>.CreateAsync(
                doctorsIQ.AsNoTracking(), pageIndex ?? 1, pageSize));

            //return View(await doctorsIQ.AsNoTracking().ToListAsync());
        }

        // GET: Doctors/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            Doctor doctor;
            if (id == null)
            {
                if (User.Identity.Name != null)
                {
                    var ownerId = UserManager.GetUserId(User);
                    doctor = await Context.Doctors
                                .Include(d => d.CommonAvailability)
                                .Include(d => d.CurrentAvailability)
                                .Include(d => d.Hospital)
                                .FirstOrDefaultAsync(m => m.OwnerID == ownerId);
                    return View(doctor);

                }
                else
                {
                    return NotFound();
                }
            }

            doctor = await Context.Doctors
                .Include(d => d.CommonAvailability)
                .Include(d => d.CurrentAvailability)
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            var isAuthorized = User.IsInRole(Constants.AdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized
                && currentUserId != doctor.OwnerID
                && !doctor.IsVerified)
            {
                return Forbid();
            }

            return View(doctor);
        }

        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsOnPost(int? id, bool status, string rejectReason)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await Context.Doctors
                .Include(d => d.CommonAvailability)
                .Include(d => d.CurrentAvailability)
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            var operation = (status == true) ? Operations.Approve : Operations.Reject;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, doctor, operation);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            doctor.IsVerified = status;
            Context.Doctors.Update(doctor);
            await Context.SaveChangesAsync();
            bool emailSentStatus;
            string message;
            var ip = HttpContext.Connection.RemoteIpAddress.ToString() + ":" + HttpContext.Connection.RemotePort.ToString();
            if (Operations.Approve.Equals(operation))
            {
                message = "<h3>Congratulations!</h3><br> Your account got approved";
                emailSentStatus = await DoctorService.SendEmail(ip, doctor.EmailId, operation.Name, message, User.Identity.Name, MessageType.AdminToUser);
            }
            else
            {
                message = "Your account is rejected. Please review and update your profile <br> Reject Reason: <br>" + rejectReason;
                emailSentStatus = await DoctorService.SendEmail(ip, doctor.EmailId, operation.Name, message, User.Identity.Name, MessageType.AdminToUser);

            }
            _logger.LogInformation($"Ip Address: {ip} \nUserName: {doctor.EmailId}\nSubject: {operation.Name}\nMessage:{message}");

            return RedirectToAction(nameof(Index));

        }
        // GET: Doctors/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //if user is not logged in
            if (User.Identity.Name == null)
            {
                return Forbid();
            }

            /*string ownerID = UserManager.GetUserId(User);
            //redirect to edit page if user already added details first time
            int doctorId = DoctorService.GetDoctorIDByOwnerID(ownerID);
            if (doctorId!=0)
            {
                return RedirectToAction(nameof(Edit), new { id = doctorId});
            }*/

            //adding details for first time
            Doctor = new Doctor
            {
                OwnerID = UserManager.GetUserId(User)
            };

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, Doctor, Operations.Create);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }


            //if (!User.IsInRole(Constants.AdministratorsRole))
            //{

            //    doctor.OwnerID = UserManager.GetUserId(User);
            //}

            #region selectList
            ViewData["CommonAvaliabilityID"] = new SelectList(Context.Availabilities, "ID", "ID");
            ViewData["CurrentAvaliabilityID"] = new SelectList(Context.Availabilities, "ID", "ID");
            ViewData["HospitalID"] = new SelectList(Context.Hospitals, "ID", "Name");

            var values = from HospitalType H in Enum.GetValues(typeof(HospitalType))
                         select new { ID = (int)H, Name = H.ToString() };
            ViewBag.HospitalType = new SelectList(values, "ID", "Name"); ;

            var district = from District d in Enum.GetValues(typeof(District))
                           select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Districts = new SelectList(district, "ID", "Name");

            var speciality = from Speciality d in Enum.GetValues(typeof(Speciality))
                             select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Speciality = new SelectList(speciality, "ID", "Name");

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
            if (!ModelState.IsValid)
            {
                SelectList();
                return View(Doctor);
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                User, Doctor,
                                                Operations.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Doctor.OwnerID = UserManager.GetUserId(User);

            //if (!User.IsInRole(Constants.AdministratorsRole))
            //{

            //    doctor.OwnerID = UserManager.GetUserId(User);
            //}
            try
            {
                #region ConditonChecks

                #endregion


                #region Initialize
                bool isTimingsValid = true;
                List<string> lstErrors = new List<string> ();
                string errorMessage ="";
                if (VerifyMinutes(Doctor.CommonAvailability.CommonDays, out errorMessage))
                {
                    
                    if(!VerifyMorningTiming(Doctor.CommonAvailability.CommonDays,out errorMessage))
                    {
                        lstErrors.Add(errorMessage);
                        isTimingsValid = false;
                    }
                    
                    if (!VerifyEveningTiming(Doctor.CommonAvailability.CommonDays, out errorMessage))
                    {
                        lstErrors.Add(errorMessage);
                        isTimingsValid = false;
                    }

                    if (Doctor.CommonAvailability.IsAvailableOnWeekend)
                    {
                        //if user selects weekend timings should be same as common days
                        if (Doctor.CommonAvailability.WeekendSameAsCommon && isTimingsValid)
                        {
                            Doctor.CommonAvailability.Weekends.MorningStartTime = Doctor.CommonAvailability.CommonDays.MorningStartTime;
                            Doctor.CommonAvailability.Weekends.MorningEndTime = Doctor.CommonAvailability.CommonDays.MorningEndTime;
                            Doctor.CommonAvailability.Weekends.EveningStartTime = Doctor.CommonAvailability.CommonDays.EveningStartTime;
                            Doctor.CommonAvailability.Weekends.EveningEndTime = Doctor.CommonAvailability.CommonDays.EveningEndTime;
                        }
                        else
                        {
                            if (!VerifyMorningTiming(Doctor.CommonAvailability.Weekends, out errorMessage, "Weekend Morning"))
                            {
                                lstErrors.Add(errorMessage);
                                isTimingsValid = false;
                            }

                            if (!VerifyEveningTiming(Doctor.CommonAvailability.Weekends, out errorMessage, "Weekend Evening"))
                            {
                                lstErrors.Add(errorMessage);
                                isTimingsValid = false;
                            }
                        }
                    }
                }
                else
                {
                    lstErrors.Add(errorMessage);
                    isTimingsValid = false;
                }

                if (!isTimingsValid)
                {
                    SelectList();
                    ViewBag.ErrorList = lstErrors;
                    return View(Doctor);
                }
                

                Doctor.CommonAvailability.AvailabilityType = Doctor.RegNumber + "_Common";
                #endregion


                if (Doctor.HospitalID != 0)
                {
                    Doctor.Hospital = null;
                    //_context.Entry(doctor.Hospital).State = EntityState.Unchanged;
                }
                Context.Doctors.Add(Doctor);

                await Context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError("Error in Doctors/Create Action" + e.Message);
            }


            SelectList();
            return View(Doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var doctor = await Context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                  User, doctor,
                                                  Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            //ViewData["CommonAvaliabilityID"] = new SelectList(Context.Availabilities, "ID", "ID", doctor.CommonAvaliabilityID);
            //ViewData["CurrentAvaliabilityID"] = new SelectList(Context.Availabilities, "ID", "ID", doctor.CurrentAvaliabilityID);
            //ViewData["HospitalID"] = new SelectList(Context.Hospitals, "ID", "Name", doctor.HospitalID);
            //var district = from District d in Enum.GetValues(typeof(District))
            //               select new { ID = (int)d, Name = d.ToString() };
            ////ViewBag.Districts = new SelectList(district, "ID", "Name");

            //var speciality = from Speciality d in Enum.GetValues(typeof(Speciality))
            //                 select new { ID = (int)d, Name = d.ToString() };
            //ViewBag.Speciality = new SelectList(speciality, "Name", "Name");

            #region selectList

            ViewData["HospitalID"] = new SelectList(Context.Hospitals, "ID", "Name");

            var values = from HospitalType H in Enum.GetValues(typeof(HospitalType))
                         select new { ID = (int)H, Name = H.ToString() };
            ViewBag.HospitalType = new SelectList(values, "ID", "Name"); ;


            var speciality = from Speciality d in Enum.GetValues(typeof(Speciality))
                             select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Speciality = new SelectList(speciality, "Name", "Name");
            #endregion

            return View(doctor);
        }

        // POST: Doctors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (id != Doctor.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                 User, Doctor,
                                                 Operations.Update);
                    if (!isAuthorized.Succeeded)
                    {
                        return Forbid();
                    }

                    if (Doctor.IsVerified)
                    {
                        // If the doctor details are updated after verification, 
                        // and the user cannot approve,
                        // set the status back to submitted so the update can be
                        // checked and approved.
                        var canApprove = await AuthorizationService.AuthorizeAsync(User,
                                                Doctor,
                                                Operations.Approve);

                        if (!canApprove.Succeeded)
                        {
                            Doctor.IsVerified = false;
                        }
                    }
                    Context.Update(Doctor);
                    await Context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorService.DoctorExists(Doctor.ID))
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

            #region selectList

            ViewData["HospitalID"] = new SelectList(Context.Hospitals, "ID", "Name");

            var values = from HospitalType H in Enum.GetValues(typeof(HospitalType))
                         select new { ID = (int)H, Name = H.ToString() };
            ViewBag.HospitalType = new SelectList(values, "ID", "Name"); ;


            var speciality = from Speciality d in Enum.GetValues(typeof(Speciality))
                             select new { ID = (int)d, Name = d.ToString() };
            ViewBag.Speciality = new SelectList(speciality, "Name", "Name");
            #endregion

            return View(Doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await Context.Doctors
                .Include(d => d.CommonAvailability)
                .Include(d => d.CurrentAvailability)
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                 User, doctor,
                                                 Operations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await Context.Doctors.FindAsync(id);

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                 User, doctor,
                                                 Operations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Context.Doctors.Remove(doctor);
            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        public IActionResult AvailabilityCreate()
        {
            return View();
        }

        public JsonResult GetLocations(int Pincode)
        {

            var locations2 = from location in Context.Locations
                             where location.Pincode == Pincode
                             select new { Locality = location.Locality, Dis = location.District };

            return Json(locations2.ToList());
        }

        private void SelectList()
        {
            #region selectList
            ViewData["CommonAvaliabilityID"] = new SelectList(Context.Availabilities, "ID", "ID");
            ViewData["CurrentAvaliabilityID"] = new SelectList(Context.Availabilities, "ID", "ID");
            ViewData["HospitalID"] = new SelectList(Context.Hospitals, "ID", "Name");

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
            //ViewBag.Gender = Enum.GetNames(typeof(Gender)).Cast<string>().ToList();
            #endregion
        }

        private bool VerifyMorningTiming(Availability.Timings timings ,out string errorMessage, string textToAdd = "Morning")
        {
            #region AnotherMethod
            /*string[] morningHours = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14" };
            string[] minutes = { "00", "15", "30", "45" };
            //string[] eveningHours = { "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "00" };

            //Common Morning TImings
            if (morningHours.Contains(timings.MorningStartHour)
                && minutes.Contains(timings.MorningStartMinute)
                && morningHours.Contains(timings.MorningEndHour)
                && minutes.Contains(timings.MorningEndMinute))
            {
                int morningStartHourIndex = Array.IndexOf(morningHours, timings.MorningStartHour);
                int morningStartMinuteIndex = Array.IndexOf(minutes, timings.MorningStartMinute);
                int morningEndHourIndex = Array.IndexOf(morningHours, timings.MorningEndHour);
                int morningEndMinuteIndex = Array.IndexOf(minutes, timings.MorningEndMinute);


                if (morningEndHourIndex > morningStartHourIndex)
                {
                    if (morningEndHourIndex == morningHours.Length - 1)
                    {
                        if (morningEndMinuteIndex != 0)
                        {
                            ViewBag.ErrorMessage = "Morning End Time must not exceed 14:00";
                            return false;
                            //timings.MorningEndMinute = minutes[0];
                        }
                    }
                }
                else if (morningStartHourIndex == morningEndHourIndex)
                {
                    if (morningEndMinuteIndex <= morningStartMinuteIndex)
                    {
                        ViewBag.ErrorMessage = "Morning End Minute cannot be less than (or) same as Morning Start Minute";
                        return false;
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Morning End hour should not be less than Morning start hour";
                    return false;
                }

            }
            else
            {
                ViewBag.ErrorMessage = "Select valid timing options";
                return false;
            }
            return true*/;
            #endregion
            return VerifyTimings(startTime: timings.MorningStartTime,
                        endTime: timings.MorningEndTime, textToAdd, out errorMessage, minHour: 00, maxHour: 14);
            
        }

        private bool VerifyEveningTiming(Availability.Timings timings, out string errorMessage, string textToAdd = "Evening")
        {
            #region AnotherMethod
            /*//string[] morningHours = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14" };
            string[] minutes = { "00", "15", "30", "45" };
            string[] eveningHours = { "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "00" };

            if (eveningHours.Contains(timings.EveningStartHour)
                    && minutes.Contains(timings.EveningStartMinute)
                    && eveningHours.Contains(timings.EveningEndHour)
                    && minutes.Contains(timings.EveningEndMinute))
            {
                int eveningStartHourIndex = Array.IndexOf(eveningHours, timings.EveningStartHour);
                int eveningStartMinuteIndex = Array.IndexOf(minutes, timings.EveningStartMinute);
                int eveningEndHourIndex = Array.IndexOf(eveningHours, timings.EveningEndHour);
                int eveningEndMinuteIndex = Array.IndexOf(minutes, timings.EveningEndMinute);


                if (eveningEndHourIndex < eveningStartHourIndex)
                {
                    ViewBag.ErrorMessage = "Evening End hour should not be less than Evening start hour";
                    return false;
                }
                else if (eveningStartHourIndex == eveningEndHourIndex)
                {
                    if (eveningEndMinuteIndex <= eveningStartMinuteIndex)
                    {
                        ViewBag.ErrorMessage = "Evening End Minute cannot be less than (or) same as Evening Start Minute";
                        return false;
                    }
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Select valid timing options";
                return false;
            }
            return true;*/
            #endregion

            return VerifyTimings(startTime: timings.EveningStartTime,
                        endTime: timings.EveningEndTime, textToAdd, out errorMessage, minHour: 14, maxHour: 23, maxMintute: 45);
        }

        private bool VerifyMinutes(Availability.Timings timings, out string errorMessage)
        {
            string[] minutes = { "00", "15", "30", "45" };

            if (minutes.Contains(timings.MorningStartMinute)
                    && minutes.Contains(timings.MorningEndMinute)
                    && minutes.Contains(timings.EveningStartMinute)
                    && minutes.Contains(timings.EveningEndMinute))
            {
                errorMessage = "";
                return true;
            }
            errorMessage = "Invalid Minutes";
            return false;
        }

        private bool VerifyTimings(DateTime startTime, DateTime endTime, string msg, out string errorMessage, int minHour, int maxHour, int minMintue = 00, int maxMintute = 00)
        {
            DateTime Now = DateTime.Now;
            
            DateTime minTime = new DateTime(year: Now.Year, month: Now.Month, day: Now.Day, hour: minHour, minute: minMintue, second: 0);
            DateTime maxTime = new DateTime(year: Now.Year, month: Now.Month, day: Now.Day, hour: maxHour, minute: maxMintute, second: 0);

            if (CompareDateTime(startTime, minTime) != DateTimeRelation.IsEarlier)
            {
                if (CompareDateTime(endTime, maxTime) != DateTimeRelation.IsLater)
                {
                    if(CompareDateTime(startTime,endTime) == DateTimeRelation.IsLater)
                    {
                        errorMessage = "";
                        return true;
                    }
                    errorMessage = $"{msg} Start time should not be greater than {msg} End Time";
                }
                else
                {
                    errorMessage = $"Invalid {msg} End Time";
                }
            }
            else
            {
                errorMessage = $"Invalid {msg} Start Time";
            }
            return false;
        }

        private DateTimeRelation CompareDateTime(DateTime dt1, DateTime dt2)
        {
            int result = DateTime.Compare(dt1, dt2);
            if (result == 0)
            {
                return DateTimeRelation.IsSame;
            }
            else if (result > 0)
            {
                return DateTimeRelation.IsLater;
            }
            return DateTimeRelation.IsEarlier;
        }

        private enum DateTimeRelation
        {
            IsEarlier,
            IsSame,
            IsLater
        }
    }

}