using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EscortsReady.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController1 : ControllerBase
    {
        [Authorize(AuthenticationSchemes = "Discord")]
        [HttpGet("GetToken")]
        public object GetToken()
        {
            var userid = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var issuer = Program.Configuration.GetValue<string>("Jwt:ValidIssuer");
            var audience = Program.Configuration.GetValue<string>("Jwt:ValidAudience");
            var key = Program.Configuration.GetValue<string>("Jwt:EncryptionKey");
            var skey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var cred = new SigningCredentials(skey, SecurityAlgorithms.HmacSha256);
            var permClaims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("discordId", userid),
            };
            var token = new JwtSecurityToken(
                issuer,
                audience,
                permClaims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: cred);
            var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
            return new
            {
                ApiToken = jwt_token,

            };
        }
    }
}
