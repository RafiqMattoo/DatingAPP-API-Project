using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Entities;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authrepository;
        public IConfiguration _config { get; set; }
        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            this._config = config;
            this.authrepository = authRepository;

        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserDataObject userDataObject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            userDataObject.UserName = userDataObject.UserName.ToLower();
            if (await authrepository.UserExists(userDataObject.UserName))
            {
                return BadRequest("User Already Exists in the System");
            }

            var userCreated = new User
            {
                Username = userDataObject.UserName
            };

            var createdUser = await authrepository.Register(userCreated, userDataObject.Password);
            return StatusCode(201);
        }

        [HttpGet]
        public async Task<IActionResult> Login(UserDataObject userDataObject)
        {
            var userDatatobject = await authrepository.Login(userDataObject.UserName, userDataObject.Password);
            if (userDatatobject == null)
            {
                return Unauthorized();
            }

            var claims = new[]{
                  new Claim(ClaimTypes.NameIdentifier, userDatatobject.Id.ToString()),
                  new Claim(ClaimTypes.Name , userDatatobject.Username)
              };

            var key = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Key").Value));

            var creds= new SigningCredentials (key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new    SecurityTokenDescriptor
            {
                Subject= new ClaimsIdentity(claims),
                Expires= DateTime.Now.AddDays(1),
                SigningCredentials=creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token=tokenHandler.CreateToken(tokenDescriptor);
            {

            };
            return Ok(new {
                token=tokenHandler.WriteToken(token)
            });
        }
    }
}