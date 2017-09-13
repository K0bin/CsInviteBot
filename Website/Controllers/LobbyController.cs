using CsInvite.Shared.Models;
using CsInvite.Website.Models.ViewModels.Lobby;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Website.Controllers
{
    public class LobbyController: Controller
    {
        private ApplicationDbContext db;
        private UserManager<User> userManager;

        public LobbyController(ApplicationDbContext db, UserManager<User> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        [HttpGet("~/lobby/{id?}")]
        public async Task<IActionResult> Index([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var vm = new IndexViewModel();

            var lobbies = db.Lobbies.Include(l => l.Members).Include(l => l.Owner).Include(l => l.Invites).Include(l => l.Invites).ThenInclude(i => i.Recipient);
            var lobby = lobbies.FirstOrDefault(l => l.Id == id);
            if (lobby == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home"); ;
            }

            vm.Members = lobby.Members;
            vm.Invites = lobby.Invites;
            vm.Owner = lobby.Owner;
            vm.Started = lobby.Created;

            return View(vm);
        }
    }
}
