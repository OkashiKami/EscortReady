using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using PermissionEx.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PermissionEx.Logic;
using Microsoft.Extensions.Options;

namespace PermissionEx.Controllers
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
            if (Utils.CurrentUser == null)
                Utils.CurrentUser = new DUser
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
            var guilds = await Utils.MyGuilds();
            var guild = !string.IsNullOrEmpty(id) ? guilds.Find(x => x.Id == ulong.Parse(id)) : null;
            if (guild != null)
                Utils.CurrentGuild = guild;
            return Redirect("/Home/Dashboard");
        }
        [HttpGet]
        public IActionResult DashboardDeselectGuild()
        {
            if (Utils.CurrentGuild != null)
                Utils.CurrentGuild = null;
            return Redirect("/Home/Dashboard");
        }

        [HttpGet]
        public IActionResult DashboardAddUserEntry()
        {
            var settings = Utils.CurrentGuildSettings;
            if (settings != null)
            {
                settings.UserEntries.Add(new DUser());
                Utils.CurrentGuildSettings = settings;
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