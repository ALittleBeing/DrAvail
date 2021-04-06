﻿using DrAvail.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DrAvail.Data;
using Microsoft.EntityFrameworkCore;

namespace DrAvail.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDoctorService _doctorService;
        private readonly ILogger _logger;

        public AdminController(IDoctorService doctorService, 
            ILogger<AdminController> logger,
            ApplicationDbContext context)
        {
            _doctorService = doctorService;
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> PendingApproval(int? pageIndex)
        {
            var Message = $"Administrator visited PendingApproval page at {DateTime.Now}";
            _logger.LogInformation(Message);
            return View(await _doctorService.GetDocotrsByVerification());
        }

        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> ViewMessages()
        {
            var messages = from m in _context.Messages select m;
            return View(await messages.ToListAsync());
        }
        
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> ReplyMessage(int? ID)
        {
            if(ID == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FirstOrDefaultAsync(M => M.ID == ID);

            if(message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        [Authorize(Roles = "Administrators")]
        [HttpPost, ActionName("ReplyMessage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyMessageOnPost(int? id, string response)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FirstOrDefaultAsync(M => M.ID == id);

            if (message == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(response))
            {
                return View(message);
            }

            message.DateResponded = DateTime.Now;
            message.AdminResponse = response;

            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
            Console.WriteLine("Admin Response: " + message.AdminResponse);
            return RedirectToAction(nameof(ViewMessages));
        }
    }
}
