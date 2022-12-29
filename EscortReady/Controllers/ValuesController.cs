using EscortsReady;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PermissionEx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpPost("UVRCU")]
        public async Task<object> UVRCU(int index, string? name)
        {
            var settings = await AssetDatabase.LoadAsync<Settings>(Session.CurrentGuild.source);
            if (settings != null)
            {
                var ue = (List<DUser>)settings["UserEntries"];
                var uentry = ue[index];
                uentry.vrchatUsername = name;
                ue[index] = uentry;
                settings["UserEntries"] = ue;
                await AssetDatabase.SaveAsync(Session.CurrentGuild.source, settings);
                return uentry;
            }
            return BadRequest();
        }
        [HttpPost("UDU")]
        public async Task<object> UDU(int index, ulong id, string? username, string? discriminator, string? email)
        {
            var settings = await AssetDatabase.LoadAsync<Settings>(Session.CurrentGuild.source);
            if (settings != null)
            {
                var ue = (List<DUser>)settings["UserEntries"];
                var uentry = ue[index];

                uentry.id = id;
                uentry.username = username;
                uentry.discriminator = discriminator;
                uentry.email = email;

                ue[index] = uentry;
                settings["UserEntries"] = ue;
                await AssetDatabase.SaveAsync(Session.CurrentGuild.source, settings);
                return uentry;
            }
            return BadRequest();
        }
    }
}
