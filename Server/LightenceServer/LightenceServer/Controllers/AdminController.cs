using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightenceServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LightenceServer.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using LightenceServer.Interfaces;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LightenceServer.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<LightUser> _userManager;
        private readonly SignInManager<LightUser> _signInManager;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILiveCountManager<string> _liveCountManager;
        private readonly IServerLogManager _serverLogManager;
        private readonly IHubGroupManager _hubGroupManager;

        public AdminController( 
            UserManager<LightUser> userManager, 
            SignInManager<LightUser> signInManager, 
            AppDbContext dbContext, 
            IConfiguration configuration,
            ILiveCountManager<string> liveCountManager,
            IServerLogManager serverLogManager,
            IHubGroupManager hubGroupManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _liveCountManager = liveCountManager;
            _serverLogManager = serverLogManager;
            _hubGroupManager = hubGroupManager;
        }

        // GET: Admin/Login
        [HttpGet]
        [Route("")]
        [Route("[action]")]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Login to administrator panel <para></para>
        /// Url: /admin/login | /admin
        /// </summary>
        /// <param name="loginModel">username, password</param>
        /// <returns> 200 OK | 400 Bad Request | 403 Forbidden </returns>
        [HttpPost]
        [Route("[action]")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromForm] LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginModel.Username);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        if(User.Identity.IsAuthenticated == false)
                        {
                            var login = await _signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, false, false);
                            if (login.Succeeded)
                            {
                                if (_liveCountManager.Contain(loginModel.Username) == false) _liveCountManager.Add(loginModel.Username);
                                _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.OK);
                                HttpContext.Session.SetString("JWToken", GenerateToken(user));
                                return RedirectToAction("Main");
                            }
                            else
                            {
                                _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                                ViewData["error"] = "Wrong password";
                                return View();
                            }
                        }
                        else
                        {
                            return RedirectToAction("Main");
                        }
                    }
                    else
                    {
                        _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error, "User is not an administrator");
                        ViewData["error"] = "User is not an administrator";
                        return View();
                    }
                }
                else
                {
                    ViewData["error"] = "Wrong username";
                    return View();
                }
            }
            else
            {
                ViewData["error"] = "Empty username or password";
                return View();
            }
        }

        
        /// <summary>
        /// Main page, show statistics about server status, signalR hub, identity stats
        /// </summary>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> Main()
        {
            Response.Headers.Add("Refresh", "300");
            ViewData["activeAdmins"] = _liveCountManager.Intersect((from Users in await _signInManager.UserManager.GetUsersInRoleAsync("Admin") select Users.UserName).ToList());
            ViewData["usage"] = _liveCountManager.Count();
            ViewData["maxUsage"] = 87;
            ViewData["allUsers"] = _userManager.Users.Count();
            ViewData["allUsersPremium"] = _userManager.GetUsersInRoleAsync("Premium").Result.Count();
            ViewData["allAdmins"] = _userManager.GetUsersInRoleAsync("Admin").Result.Count();
            ViewData["activeRooms"] = _hubGroupManager.Count;
            ViewData["bandwidthOut"] = 512; //Mbit/s
            ViewData["loginSuccess"] = ( from log in _dbContext.ServerLogs
                                        where ( log.Type == ServerLogType.LoginUser
                                                && log.Result == ServerLogResult.OK
                                                && log.Time >= DateTime.Now.AddMinutes(-60))
                                        select log ).ToList();
            ViewData["loginError"] = ( from log in _dbContext.ServerLogs
                                        where ( log.Type == ServerLogType.LoginUser 
                                                && log.Result == ServerLogResult.Error 
                                                && log.Time >= DateTime.Now.AddMinutes(-60))
                                        select log ).ToList();
            return View();
        }
        

        /// <summary>
        /// Logout from admin panel
        /// </summary>
        /// <returns>200 OK | 401 Unauthorized</returns>
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            _liveCountManager.Substract(User.Identity.Name ?? String.Empty);
            _serverLogManager.AddLog(ServerLogType.LogoutUser);
            return View();
        }

        [Route("[action]")]
        [HttpGet]
        public ActionResult ManageUser()
        {
            return View();
        }

        /// <summary>
        /// Managing user, can delete user
        /// </summary>
        /// <param name="username">username</param>
        /// <returns> 200 OK | 400 Bad Request | 403 Forbidden </returns>
        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> ManageUser([FromForm] string username)
        {
            if(username != null)
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    if (username != User.Identity.Name)
                    {
                        if (!await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            var roles = await _userManager.GetRolesAsync(user);
                            await _userManager.RemoveFromRolesAsync(user, roles);
                            await _userManager.DeleteAsync(user);
                            _liveCountManager.Substract(username);
                        }
                        else ViewData["error"] = "Cannot delete admin!";
                    }
                    else
                    {
                        ViewData["error"] = "Cannot delete yourself!";
                    }
                }
                else
                {
                    ViewData["error"] = "Wrong username";
                }
            }
            else
            {
                ViewData["error"] = "Empty username";
            }
            return View();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> ManageAdmin()
        {
            ViewData["allAdmins"] = (from admin in await _userManager.GetUsersInRoleAsync("Admin")
                                orderby admin.UserName ascending   
                                select admin.UserName).ToList();
            return View();
        }

        /// <summary>
        /// Add or remove from admin group. Only one string [ add | remove ] is not null here
        /// </summary>
        /// <param name="username">username </param>
        /// <param name="add">add to admin group</param>
        /// <param name="remove">remove from admin group. </param>
        /// <returns>200 OK | 400 Bad request | 401 Unauthorized | 403 Forbidden</returns>
        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult> ManageAdmin(string username, string add, string remove)
        {
            if (username != null)
            {
                if (string.IsNullOrEmpty(add) == false)
                {
                    var user = await _userManager.FindByNameAsync(username);
                    if (user != null)
                    {
                        if (!await _userManager.IsInRoleAsync(user, "Admin")) await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else ViewData["error"] = "Wrong username";
                }
                if (string.IsNullOrEmpty(remove) == false)
                {
                    var user = await _userManager.FindByNameAsync(username);
                    if (user != null)
                    {
                        if(username != User.Identity.Name)
                        {
                            if (await _userManager.IsInRoleAsync(user, "Admin")) await _userManager.RemoveFromRoleAsync(user, "Admin");
                        }
                        else
                        {
                            ViewData["error"] = "Cannot delete yourself";
                        }  
                    }
                    else ViewData["error"] = "Wrong username";
                }
            }
            else
            {
                ViewData["error"] = "Empty username";
            }

            ViewData["allAdmins"] = (from admin in await _userManager.GetUsersInRoleAsync("Admin")
                                     orderby admin.UserName ascending
                                     select admin.UserName).ToList();
            return View();
        }

        #region Token generator
        private string GenerateToken(LightUser user)
        {
            var timeNow = DateTime.Now;

            var claims = new List<Claim>
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                 new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(JwtRegisteredClaimNames.Iat, timeNow.ToString()),
            };

            foreach (var item in _userManager.GetRolesAsync(user).Result)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }
            claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, $"{user.FirstName} {user.LastName}"));

            var tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Token:Key"]));
            var signingCredentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha384);
            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: timeNow.AddSeconds(-1),
                expires: timeNow.AddMinutes(double.Parse(_configuration["Token:Lifetime"])),
                audience: _configuration["Token:Audience"],
                issuer: _configuration["Token:Issuer"]
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);

        }
        #endregion
    }
}
