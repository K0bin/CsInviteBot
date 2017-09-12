using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Models;
using CsInvite.Models.ViewModels.Friends;
using Microsoft.AspNetCore.Identity;

namespace CsInvite.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<User> userManager;

        public FriendsController(ApplicationDbContext db, UserManager<User> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(IndexViewModel viewModel = null)
        {
            if (viewModel == null)
            {
                viewModel = new IndexViewModel();
            }

            var user = await userManager.GetUserAsync(User);

            var friends = db.Friends.Where(friend => friend.UserId == user.Id);
            viewModel.Friends = new List<Friend>();
            foreach (var friend in friends)
            {
                viewModel.Friends.Add(friend);
            }
            return View(viewModel);
        }

        public IActionResult Search(SearchViewModel viewModel = null)
        {
            if (!string.IsNullOrWhiteSpace(viewModel?.Query))
            {
                var users = db.Users.Where(user => user.UserName.Contains(viewModel.Query));
                viewModel.Users = new List<Models.User>();
                foreach (var user in users)
                {
                    viewModel.Users.Add(user);
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(string id, string query = null)
        {
            var user = await userManager.GetUserAsync(User);
            if (String.IsNullOrWhiteSpace(id) || id == user.Id)
            {
                return RedirectToAction(nameof(Search), "Friends", new SearchViewModel { Query = query });
            }
            db.Friends.Add(new Friend
            {
                UserId = user.Id,
                FriendUserId = id,
                Priority = 0,
                LastInvite = new DateTime()
            });
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
