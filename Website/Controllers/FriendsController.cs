using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Website.Models;
using CsInvite.Website.Models.ViewModels.Friends;
using Microsoft.AspNetCore.Identity;
using CsInvite.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using CsInvite.Website.Models.Steam;
using CsInvite.Website.Extensions;

namespace CsInvite.Website.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<User> userManager;
        private Steam steam;

        public FriendsController(ApplicationDbContext db, UserManager<User> userManager, Steam steam)
        {
            this.db = db;
            this.userManager = userManager;
            this.steam = steam;
        }

        public async Task<IActionResult> Index(IndexViewModel viewModel = null)
         {
            if (viewModel == null)
            {
                viewModel = new IndexViewModel();
            }

            var userId = userManager.GetUserId(User);
            var users = db.Users.Include(u => u.Friends).ThenInclude(friend => friend.OtherUser);
            var user = users.FirstOrDefault(u => u.Id == userId);
            viewModel.Friends = user?.Friends ?? new List<Friend>();
            return View(viewModel);
        }

        public async Task<IActionResult> Search(SearchViewModel viewModel = null)
        {
            if (!string.IsNullOrWhiteSpace(viewModel?.Query))
            {
                var userId = userManager.GetUserId(User);
                var users = db.Users.Include(u => u.Friends);
                var user = users.FirstOrDefault(u => u.Id == userId);
                viewModel.Users = db.Users.Where(u => 
                                        u.UserName.Contains(viewModel.Query) 
                                        && u.Id != user.Id 
                                        && !user.Friends.Any(f => f.OtherUserId == u.Id)
                                    ).ToList();
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
                OtherUserId = id,
                Priority = 0
            });
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Import()
        {
            var dbUsers = db.Users.Include(u => u.IsInFriendsListOf);

            var user = await userManager.GetUserAsync(User);
            var friends = await steam.GetSteamFriendsList(user.SteamId);
            if (friends == null)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var friend in friends)
            {
                if (friend == null)
                {
                    continue;
                }

                var friendUser = dbUsers.FirstOrDefault(u => u.SteamId == friend.SteamId);
                if (friendUser == null)
                {
                    var steamUser = await steam.GetSteamPlayer(friend.SteamId);
                    friendUser = steamUser.ToUser();
                    if (friendUser == null)
                    {
                        continue;
                    }
                    await userManager.CreateAsync(friendUser);
                }

                if (!(friendUser.IsInFriendsListOf?.Any(f => f.UserId == user.Id) ?? false))
                {
                    var friendShip = new Friend
                    {
                        UserId = user.Id,
                        OtherUserId = friendUser.Id,
                    };
                    db.Friends.Add(friendShip);
                }
            }
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
