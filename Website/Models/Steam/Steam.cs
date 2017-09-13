using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CsInvite.Website.Models.Steam
{
    public class Steam
    {
        private string apiKey;
        public Steam(IConfiguration configuration)
        {
            this.apiKey = configuration["SteamApiKey"];
        }

        public async Task<SteamPlayerSummary> GetSteamPlayer(ulong steamId)
        {
            SteamPlayerSummary player = null;
            using (var client = new HttpClient())
            {
                // Query steam user summary endpoint
                var response = await client.GetAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={steamId}");

                // If result not OK, throw error
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException)
                {
                    return null;
                }

                // Deserialize json and return player DTO
                var stringResponse = await response.Content.ReadAsStringAsync();

                // Get display name
                player = JsonConvert.DeserializeObject<SteamPlayerSummaryRootObject>(stringResponse).Response.Players[0];
            }
            return player;
        }

        public async Task<List<SteamFriend>> GetSteamFriendsList(ulong steamId)
        {
            List<SteamFriend> list = null;
            using (var client = new HttpClient())
            {
                // Query steam user summary endpoint
                var response = await client.GetAsync($"http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key={apiKey}&steamid={steamId}&relationship=friend");

                // If result not OK, throw error
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    return null;
                }

                // Deserialize json and return player DTO
                var stringResponse = await response.Content.ReadAsStringAsync();

                // Get display name
                list = JsonConvert.DeserializeObject<SteamFriendsRootObject>(stringResponse).FriendsList.Friends;
            }
            return list;
        }
    }
}
