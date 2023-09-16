using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication_13.Models;
using WebApplication_13.Models.Authentication.Login;
using WebApplication_13.Models.Authentication.Register;

namespace WebApplication_13.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
       
        public AuthenticationController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser, string role)
        {
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist != null) {
                Console.WriteLine(role);
                return StatusCode(StatusCodes.Status403Forbidden,
                    new Response { Status = "Error", StatusMessage = "User already exists" });
            }
            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username
            };
            if(await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password);

                if (!result.Succeeded)
                {
                    Console.WriteLine(role, user);
                    return StatusCode(StatusCodes.Status500InternalServerError,
                      new Response { Status = "Error", StatusMessage = "User failed to Create" });
                }
                var token=await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email });
                     await _userManager.AddToRoleAsync(user, role);
                    return StatusCode(StatusCodes.Status201Created,
                        new Response { Status = "Success", StatusMessage = "User Created Sucessfully && timepaas mail sent" });
               
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  new Response { Status = "Error", StatusMessage = "Role does not exist" });
            }
           
           
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user=await _userManager.FindByEmailAsync(email); 
            if(user != null) { 
            var result=await _userManager.ConfirmEmailAsync(user, token);
                if(result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, 
                        new Response { Status="Success", StatusMessage= "Email verified" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", StatusMessage = "User does not exist" });

        }
        [HttpPost("LoginUser")]
        public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
        {
            var user = await _userManager.FindByNameAsync(loginUser.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginUser.Password)) {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }
                var jwtToken = GetToken(authClaims);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo
                });
        }
            return Unauthorized();
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires:DateTime.Now.AddHours(3),
                claims:authClaims,
                signingCredentials:new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256));
            return token;
        }
    }
}
