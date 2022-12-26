using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PermissionEx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpPost("UVRCU")]
        public object UVRCU(int index, string? name)
        {
            var settings = Utils.CurrentGuildSettings;
            if (settings != null)
            {
                var uentry = settings.UserEntries[index];
                uentry.vrchatUsername = name;
                settings.UserEntries[index] = uentry;
                Utils.CurrentGuildSettings = settings;
                return uentry;
            }
            return BadRequest();
        }
        [HttpPost("UDU")]
        public object UDU(int index, ulong id, string? username, string? discriminator, string? email)
        {
            var settings = Utils.CurrentGuildSettings;
            if (settings != null)
            {
                var uentry = settings.UserEntries[index];
                uentry.id = id;
                uentry.username = username;
                uentry.discriminator = discriminator;
                uentry.email = email;

                settings.UserEntries[index] = uentry;
                Utils.CurrentGuildSettings = settings;
                return uentry;
            }
            return BadRequest();
        }
    }
}
