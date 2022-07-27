using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using LightenceServer.Data;
using LightenceServer.Interfaces;
using LightenceServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Newtonsoft.Json;
using Microsoft.Rest;
using System.IO;
using Microsoft.AspNetCore.Mvc.Formatters;


namespace LightenceServer.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize]
    public class IdentityController : Controller
    {
        private readonly UserManager<LightUser> _userManager;
        private readonly SignInManager<LightUser> _signInManager;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IProductKeyManager _productKeyManager;
        private readonly IServerLogManager _serverLogManager;
        private readonly ILiveCountManager<string> _liveCountManager;

        public IdentityController( 
            UserManager<LightUser> userManager, 
            SignInManager<LightUser> signInManager,
            AppDbContext dbContext,
            IConfiguration configuration, 
            IProductKeyManager productKeyManager,
            IServerLogManager serverLogManager,
            ILiveCountManager<string> liveCountManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _productKeyManager = productKeyManager;
            _serverLogManager = serverLogManager;
            _liveCountManager = liveCountManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Testing()
        {
            return Ok(new { Text = "How Info Gain will be changed, if 10-2?" });
        }

        /// <summary>
        /// Login user to system <para></para>
        /// Url: /identity/login 
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns>200 OK | 400 Bad Request </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginModel.Username);

                if(await _userManager.IsEmailConfirmedAsync(user) == true)
                {
                    var result = await _signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        if (_liveCountManager.Contain(loginModel.Username) == false) _liveCountManager.Add(loginModel.Username);

                        _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.OK);

                        return Ok(new ResponseMessageModel
                        {
                            Error = "",
                            Content = new { Token = GenerateToken(user) }
                        });
                    }
                    else
                    {
                        _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                        return BadRequest(new ResponseMessageModel
                        {
                            Error = "Bad login or password",
                            Content = null
                        });
                    }
                }
                else // for future email confirmation
                {
                    _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                    return BadRequest(new ResponseMessageModel
                    {
                        Error = "Email is not confirmed",
                        Content = null
                    });
                }
            }
            else
            {
                _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                });
            }
        }

        /// <summary>
        /// Logout user from system <para></para>
        /// Url: /identity/logout
        /// </summary>
        /// <returns>200 OK | 401 Unauthorized </returns>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
           await _signInManager.SignOutAsync();
           _liveCountManager.Substract(User.Identity.Name ?? String.Empty);
           _serverLogManager.AddLog(ServerLogType.LogoutUser, ServerLogResult.OK);
           return Ok(new ResponseMessageModel { Error = "", Content = null});
        }

        /// <summary>
        /// Register new user and login to system. Login has to be an e-mail.<para></para>
        /// Password policy: 8+ characters, 1+ Upper letter, 1+ lower letter, 1+ number <para></para>
        /// Url: /identity/register
        /// </summary>
        /// <param name="registerModel">username, password, firstname and lastname</param>
        /// <returns>200 OK | 400 Bad Request </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (ModelState.IsValid)
            {
                var user = new LightUser
                {
                    UserName = registerModel.UserName,
                    Email = registerModel.UserName,
                    FirstName = registerModel.FirstName,
                    LastName = registerModel.LastName
                };

                var result = await _userManager.CreateAsync(user, registerModel.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    _serverLogManager.AddLog(ServerLogType.RegisterUser, ServerLogResult.OK);
                    
                    var _token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var link = Url.Action(
                        action: "ConfirmEmail",
                        controller: "Identity",
                        values: new { username = user.UserName, token = _token },
                        protocol: HttpContext.Request.Scheme);

                    SendMail(link, user.Email, "verify Your email");

                    return StatusCode(201, new ResponseMessageModel
                    {
                        Error = "",
                        Content = new { Text = "account created" }
                        //Content = new {Text = "Sent verification email"}
                    });
                }
                else
                {
                    _serverLogManager.AddLog(ServerLogType.RegisterUser, ServerLogResult.Error);
                    return BadRequest(new ResponseMessageModel
                    {
                        Error = result.Errors.First().ToString(),
                        Content = null
                    });
                }
            }
            else
            {
                _serverLogManager.AddLog(ServerLogType.RegisterUser, ServerLogResult.Error);
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                });
            }

        }

        /// <summary>
        /// Update information about user: first name or/and last name. Cannot change password here<para></para>
        /// URL: /identity/update
        /// </summary>
        /// <param name="updateAccount">firstname, lastname</param>
        /// <returns>200 OK | 400 Bad Request | 401 Unauthorized </returns>
        [HttpPut]
        public async Task<IActionResult> Update(UpdateAccountModel updateAccount)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name ?? User.Claims.Where(c => c.Properties.ContainsKey("unique_name"))
                                                                                                .Select(c => c.Value).FirstOrDefault());
                if (user != null)
                {
                    user.FirstName = updateAccount.FirstName ?? user.FirstName;
                    user.LastName = updateAccount.LastName ?? user.LastName;

                    await _userManager.UpdateAsync(user);
                    _serverLogManager.AddLog(ServerLogType.UpdateUser, ServerLogResult.OK);

                    return Ok(new ResponseMessageModel
                    {
                        Error = "",
                        Content = new { Text = "Succesfully changed info" }
                    });
                }
                _serverLogManager.AddLog(ServerLogType.UpdateUser, ServerLogResult.Error);
                return BadRequest(new ResponseMessageModel
                {
                    Error = "Wrong login",
                    Content = null
                });
            }
            else
            {
                _serverLogManager.AddLog(ServerLogType.UpdateUser, ServerLogResult.Error);
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                });
            }
        }

        /// <summary>
        /// Delete user from system. Deletes all its roles, sign out. <para></para>
        /// URL: /identity/delete
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name ?? User.Claims.Where(c => c.Properties.ContainsKey("unique_name"))
                                                                                               .Select(c => c.Value).FirstOrDefault());
                //logout 
                var roles = await _userManager.GetRolesAsync(user);
                await _signInManager.SignOutAsync();
                await _userManager.RemoveFromRolesAsync(user, roles);

                _liveCountManager.Substract(User.Identity.Name ?? String.Empty);

                //remove biometric profile
                var bio = _dbContext.BiometricLogins.FirstOrDefault(x => x.UserName == user.Email);
                if (bio != null)
                {
                    _dbContext.BiometricLogins.Remove(bio);
                    _dbContext.SaveChanges();
                }

                //delete account
                await _userManager.DeleteAsync(user);
                _serverLogManager.AddLog(ServerLogType.DeleteUser, ServerLogResult.OK);

                return Ok(new ResponseMessageModel
                {
                    Error = "",
                    Content = new { Text = "Deleted" }
                });
            }
            else
            {
                _serverLogManager.AddLog(ServerLogType.DeleteUser, ServerLogResult.Error);
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                });
            }
        }

        /// <summary>
        /// Change user password <para></para>
        /// Password policy: 8+ characters, 1+ Upper letter, 1+ lower letter, 1+ number <para></para>
        /// URL: /identity/changepass
        /// </summary>
        /// <param name="changePassword">Old valid password and new password</param>
        /// <returns>200 OK | 400 Bad Request | 401 Unauthorized</returns>
        [HttpPut]
        public async Task<IActionResult> Changepass([FromBody] ChangePasswordModel changePassword)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name ?? User.Claims.Where(c => c.Properties.ContainsKey("unique_name"))
                                                                                               .Select(c => c.Value).FirstOrDefault());

                var result = await _userManager.ChangePasswordAsync(user, changePassword.OldPassword, changePassword.NewPassword);
                if (result.Succeeded)
                {
                    _serverLogManager.AddLog(ServerLogType.ChangePass, ServerLogResult.OK);
                    return Ok(new ResponseMessageModel
                    {
                        Error = "",
                        Content = new { Text = "password has been changed" }
                    });
                }
                else
                {
                    _serverLogManager.AddLog(ServerLogType.ChangePass, ServerLogResult.Error);
                    return BadRequest(new ResponseMessageModel
                    {
                        Error = result.Errors.ToString(),
                        Content = null
                    });
                }
            }
            else
            {
                _serverLogManager.AddLog(ServerLogType.ChangePass, ServerLogResult.Error);
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                });
            }
        }

        /// <summary>
        /// Add premium to user <para></para>
        /// URL: /identity/premium
        /// </summary>
        /// <param name="appkey">Product key has 25 chars, only letters; can have '-' chars</param>
        /// <returns>200 OK | 400 Bad Request | 401 Unauthorized</returns>
        [HttpPost]
        public async Task<IActionResult> Premium([FromBody] KeyModel appkey)
        {
            if (ModelState.IsValid)
            {
                var key = appkey.Appkey.Replace("-", String.Empty); //deleting '-' characters from product key string

                var user = await _userManager.FindByNameAsync(User.Identity.Name ?? User.Claims.Where(c => c.Properties.ContainsKey("unique_name"))
                                                                                                   .Select(c => c.Value).FirstOrDefault());

                var isValid = _productKeyManager.VerifyKey(key);
                if (isValid)
                {
                    if (await _userManager.IsInRoleAsync(user, "Premium") == false)
                    {
                        var result = await _productKeyManager.AddKeyAsync(key);
                        if (result == 0)
                        {
                            await _userManager.AddToRoleAsync(user, "Premium");
                            return Ok(new ResponseMessageModel
                            {
                                Error = "",
                                Content = new { Text = "Premium account is activated" }
                            });
                        }
                        else return BadRequest(new ResponseMessageModel
                        {
                            Error = "Produkt key is used maximum times",
                            Content = null
                        });
                    }
                    else return BadRequest(new ResponseMessageModel
                    {
                        Error = "User already has premium account",
                        Content = null
                    });
                }
                else return BadRequest(new ResponseMessageModel
                {
                    Error = "Product Key is invalid",
                    Content = null
                });
            }
            else
            {
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                });
            }
        }

        /// <summary>
        /// Send reset password e-mail 
        /// URL: /identity/resetpassword
        /// </summary>
        /// <param name="userName">user who forgot the password</param>
        /// <returns>200 OK | 400 Bad Request</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] object userName)
        {
            if(ModelState.IsValid)
            {
                var _userName = userName.ToString() ?? string.Empty;
                var user = await _userManager.FindByNameAsync(_userName);
                if(user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var link = Url.Action(
                        action: "ConfirmResetPassword",
                        controller: "Identity",
                        values: new { username = _userName, token },
                        protocol: HttpContext.Request.Scheme);

                    SendMail(link, user.Email, "reset password");

                    return Ok(new ResponseMessageModel
                    {
                        Error = null,
                        Content = "sent mail"
                    });
                }
                else
                {
                    return BadRequest(new ResponseMessageModel
                    {
                        Error = "Wrong username",
                        Content = null
                    });
                }
            }
            else
            {
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                });
            }
        }

        
        #region Vision functions

        /// <summary>
        /// Add user face <para/>
        /// Use Cognitive Services to identity face and save
        /// URL: /identity/addvision
        /// </summary>
        /// <returns>200 OK | 400 Bad Request</returns>
        [HttpPost]
        public async Task<IActionResult> AddVision([FromBody] VisionLoginModel visionLoginModel)
        {
            // CHECK if VisionLoginModel has good types (byte[] -> string e.g)
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(visionLoginModel.userName);
                if (user != null)
                {
                    MemoryStream checkFace = new MemoryStream(visionLoginModel.data);
                    FaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_configuration["CognitiveServices:SubscriptionKey"]))
                    {
                        Endpoint = _configuration["CognitiveServices:Endpoint"]
                    };

                    var modelList = await faceClient.Face.DetectWithStreamAsync(checkFace, recognitionModel: "recognition_03");
                    if (modelList.Count != 1)
                    {
                        
                        return BadRequest(new ResponseMessageModel
                        {
                            Error = $"wroong number of faces: {modelList.Count}",
                            Content = null
                        });
                    }
                    else
                    {
                        var profile = _dbContext.BiometricLogins.FirstOrDefault(x=> x.UserName == visionLoginModel.userName);
                        if (profile == null)
                        {
                            _dbContext.BiometricLogins.Add(new LightFaceModel
                            {
                                UserName = visionLoginModel.userName,
                                FaceData = visionLoginModel.data
                            });
                        }
                        else
                        {
                            profile.FaceData = visionLoginModel.data;
                        }
                        var cnt = _dbContext.SaveChanges();
                        if (cnt > 0)
                        {
                            return Ok(new ResponseMessageModel
                            {
                                Error = "",
                                Content = new { Text = "OK" }
                            });
                        }
                        else
                        {
                            return BadRequest(new ResponseMessageModel
                            {
                                Error = "Cannot create biometric login",
                                Content = null
                            });
                        }
                    }
                }
                else
                {
                    return BadRequest(new ResponseMessageModel
                    {
                        Error = "wrong username",
                        Content = null
                    });
                }
            }
            else
            {
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                }) ;
            }

        }

        /// <summary>
        /// Login user with face recognition <para/>
        /// Use Cognitive Services to confirm face
        /// Compare two faces and get result
        /// URL: /identity/loginvision
        /// </summary>
        /// <returns>200 OK | 400 Bad Request</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginVision([FromBody] VisionLoginModel visionLoginModel)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(visionLoginModel.userName);
                if (user != null)
                {
                    if(await _userManager.IsEmailConfirmedAsync(user) == true)
                    {
                        var face = _dbContext.BiometricLogins.FirstOrDefault(x => x.UserName == visionLoginModel.userName);
                        if (face != null)
                        {
                            MemoryStream modelFace = new MemoryStream(face.FaceData);
                            MemoryStream checkFace = new MemoryStream(visionLoginModel.data);
                            FaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_configuration["CognitiveServices:SubscriptionKey"]))
                            {
                                Endpoint = _configuration["CognitiveServices:Endpoint"]
                            };

                            var modelList = await faceClient.Face.DetectWithStreamAsync(modelFace, recognitionModel: "recognition_03");
                            var checkList = await faceClient.Face.DetectWithStreamAsync(checkFace, recognitionModel: "recognition_03");

                            if (modelList.Count != 1)
                            {
                                _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                                return BadRequest(new ResponseMessageModel
                                {
                                    Error = $"too many faces {checkList.Count}",
                                    Content = null
                                });
                            }

                            var modelID = modelList[0].FaceId ?? Guid.Empty;
                            var checkID = checkList[0].FaceId ?? Guid.Empty;
                            var prediction = await faceClient.Face.VerifyFaceToFaceAsync(modelID, checkID);
                            if (prediction.Confidence > 0.9)
                            {
                                if (_liveCountManager.Contain(visionLoginModel.userName) == false) _liveCountManager.Add(visionLoginModel.userName);

                                _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.OK);
                                await _signInManager.SignInAsync(user, false);
                                return Ok(new ResponseMessageModel
                                {
                                    Error = "",
                                    Content = new { Token = GenerateToken(user) }
                                });
                            }
                            else
                            {
                                _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                                return BadRequest(new ResponseMessageModel
                                {
                                    Error = "face was not recognized ",
                                    Content = null
                                });
                            }
                        }
                        else
                        {
                            _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                            return BadRequest(new ResponseMessageModel
                            {
                                Error = "user doesn't have biometric profile",
                                Content = null
                            });
                        }
                    }
                    else
                    {
                        _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                        return BadRequest(new ResponseMessageModel
                        {
                            Error = "email is not verified",
                            Content = null
                        });
                    }
                }
                else
                {
                    _serverLogManager.AddLog(ServerLogType.LoginUser, ServerLogResult.Error);
                    return BadRequest(new ResponseMessageModel
                    {
                        Error = "wrong username",
                        Content = null
                    });
                }
            }
            else
            {
                return BadRequest(new ResponseMessageModel
                {
                    Error = ModelState.Values.First().ToString(),
                    Content = null
                });
            }
            
        }

        /// <summary>
        /// Delete biometric profile
        /// </summary>
        /// <returns>200 OK | 400 Bad Request</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteVision()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name ?? User.Claims.Where(c => c.Properties.ContainsKey("unique_name"))
                                                                                               .Select(c => c.Value).FirstOrDefault());

            var bio = _dbContext.BiometricLogins.FirstOrDefault(x => x.UserName == user.Email);
            if (bio != null)
            {
                _dbContext.BiometricLogins.Remove(bio);
                _dbContext.SaveChanges();

                return Ok(new ResponseMessageModel
                {
                    Error = "",
                    Content = new { Text = "Successfully deleted biometric profile" }
                });
            }
            else
            {
                return BadRequest(new ResponseMessageModel
                {
                    Error = "No biometric profile found",
                    Content = null
                });
            }
        }

        #endregion
        
        #region Endpoints not called by client app

        /// <summary>
        /// Confirm email from link sent to user
        /// </summary>
        /// <param name="username">userName</param>
        /// <param name="token">verification token</param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail([FromQuery]string username,[FromQuery] string token)
        {
            var user = await _userManager.FindByNameAsync(username);
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded) ViewData["result"] = "Email succesfully confirmed";
            else ViewData["result"] = "Error to confirm email";

            return View();

        }

        /// <summary>
        /// Show reset password page, need to input new password
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmResetPassword([FromQuery] string username, [FromQuery] string token)
        {
            var model = new ResetPasswordModel { UserName = username, Token = token };
            return View(model);
        }

        /// <summary>
        /// Reset password confirmation  
        /// </summary>
        /// <param name="resetPasswordModel">username, confirmation token and password</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmResetPassword([FromForm] ResetPasswordModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if(result.Succeeded) ViewData["result"] = "Succesfully reset password";
            return View("ConfirmPage");
        }

        #endregion

        /// <summary>
        /// Generating new token to user.
        /// </summary>
        /// <param name="user">User which will get new token</param>
        /// <returns></returns>
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

        /// <summary>
        /// Send mail function 
        /// </summary>
        /// <param name="link">action link to pass</param>
        /// <param name="userEmail">user who receive email</param>
        /// <param name="msg">message content</param>
        private void SendMail(string link, string userEmail, string msg)
        {
            SmtpClient client = new SmtpClient("smtp-mail.outlook.com")
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(_configuration["Smtp:Mail"], _configuration["Smtp:Password"])
            };

            MailMessage message = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:Mail"]),
                Subject = $"Lightence - {msg}",
                Body = $"<div style='font-family:sans-serif; color:#383f51; line-height:110%'> Please click on this <a href = { link } style = 'color:#2196f3'> link </a> to { msg }. <p>If You don't expect this e-mail, please ignore it.</p> <p style='font-size:11pt'>Lightence {System.DateTime.Now.Year}</p> </div>",
                IsBodyHtml = true
            };
            message.To.Add(userEmail);
            client.Send(message);
        }
    }
}


/* ---------------< TEMPLATES JSON RESPONSE >-----------------
 * 
 * { 
 *   error : "every string",
 *   content : {every object}
 * }
 * 
 * USED IN PROJECT (CAN MIX)
 * 
 * {
 *   error : "error code ",
 *   content : 
 *      {
 *          token: "token string"
 *      }
 * 
 * }
 * 
 * {
 *   error : "error code",
 *   content : 
 *      {
 *          text: "info"
 *      }
 * 
 * }
 * 
 * {
 *   error : "",
 *   content : null
 * }
 * 
 * 
 */ 