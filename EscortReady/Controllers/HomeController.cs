using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using EscortsReady.Models;
using EscortReady;

namespace EscortsReady.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public Settings Settings { get; }

        public HomeController(IOptionsSnapshot<Settings> options, ILogger<HomeController> logger)
        {
            Settings = options.Value;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult Dashboard()
        {
            if (Session.CurrentUser == null)
                Session.CurrentUser = new DUser
                {
                    id = ulong.Parse((string)User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value),
                    username = User.Claims.First(x => x.Type == ClaimTypes.Name).Value,
                    discriminator = User.Claims.First(x => x.Type == ClaimTypes.Hash).Value,
                    email = User.Claims.First(x => x.Type == ClaimTypes.Email).Value
                };
            return View();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> DashboardSelectGuild(string id)
        {
            var guilds = await DiscordService.MyGuildsAsync();
            var guild = !string.IsNullOrEmpty(id) ? guilds.Find(x => x.Id == ulong.Parse(id)) : null;
            if (guild != null)
                Session.CurrentGuild = guild;
            return Redirect("/Home/Dashboard");
        }
        [HttpGet]
        public IActionResult DashboardDeselectGuild()
        {
            if (Session.CurrentGuild != null)
                Session.CurrentGuild = null;
            return Redirect("/Home/Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> DashboardAddUserEntry()
        {
            var settings = await  AssetDatabase.LoadAsync<Settings>(Session.CurrentGuild.source);
            if (settings != null)
            {
                var ue = (List<DUser>)settings["UserEntries"];
                    ue.Add(new DUser());
                await AssetDatabase.SaveAsync(Session.CurrentGuild.source, settings);
            }
            return Redirect("/Home/Dashboard");
        }
        [HttpGet]
        public async Task<IActionResult> DashboardRemoveUserEntry(string id)
        {
            var settings = await AssetDatabase.LoadAsync<Settings>(Session.CurrentGuild.source);
            if (settings != null)
            {
                var ue = (List<DUser>)settings["UserEntries"];
                ue.RemoveAt(int.Parse(id));
                await AssetDatabase.SaveAsync(Session.CurrentGuild.source, settings);
            }
            return Redirect("/Home/Dashboard");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}