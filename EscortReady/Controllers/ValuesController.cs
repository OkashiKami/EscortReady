using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EscortsReady.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpPost("UVRCU")]
        public async Task<object> UVRCUAsync(int index, string? name)
        {
            var settings = await Settings.LoadAsync(Session.CurrentGuild.source);
            if (settings != null)
            {
                var ue = (List<DUser>)settings["UserEntries"];
                ue[index].vrchatUsername = name;
                settings["UserEntries"] = ue;
                await Settings.SaveAsync(Session.CurrentGuild.source, settings);
                return ue[index];
            }
            return BadRequest();
        }
        [HttpPost("UDU")]
        public async Task<object> UDUAsync(int index, ulong id, string? username, string? discriminator, string? email)
        {
            var settings = await Settings.LoadAsync(Session.CurrentGuild.source);
            if (settings != null)
            {
                var ue = (List<DUser>)settings["UserEntries"];
                ue[index].id = id;
                ue[index].username = username;
                ue[index].discriminator = discriminator;
                ue[index].email = email;
                settings["UserEntries"] = ue;
                await Settings.SaveAsync(Session.CurrentGuild.source, settings);
                return ue[index];
            }
            return BadRequest();
        }
    }
}
