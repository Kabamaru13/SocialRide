using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using SocialRide.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using SocialRide.Services;
using SocialRide.Dtos;
using SocialRide.Models;

namespace SocialRide.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Facebook or Google Login. Non-existed users will be registered first, existed users will just login.
        /// </summary>
        /// <returns>The user's access token and refresh token</returns>
        /// <param name="userDto">User - Id, Firstname, Lastname, Email, Prefix, Phone, Avatar</param>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserDto userDto)
        {
            // map dto to entity
            var user = _mapper.Map<User>(userDto);

            try
            {
                // save 
                user = _userService.Create(user);
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new ResultData()
                {
                    data = new { },
                    error = new Error() { errorCode = (int)ErrorCode.RegistrationGeneric, message = ex.Message }
                });
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id)
                        //user.Username == "Kabamaru" ? new Claim("AdminBadge", "") : null
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var refreshDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id),
                        new Claim("RefreshBadge", user.Id)
                    }),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                SecurityToken accessToken = tokenHandler.CreateToken(tokenDescriptor);
                SecurityToken refreshToken = tokenHandler.CreateToken(refreshDescriptor);

                var tokenString = tokenHandler.WriteToken(accessToken);
                var refreshString = tokenHandler.WriteToken(refreshToken);

                return Ok(new ResultData()
                {
                    data = new
                    {
                        access_token = tokenString,
                        token_type = "bearer",
                        expires_in = "3600",
                        refresh_token = refreshString
                    },
                    error = new Error() { errorCode = (int)ErrorCode.NoError, message = "" }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResultData()
                {
                    data = new { },
                    error = new Error() { errorCode = (int)ErrorCode.AuthenticationGeneric, message = ex.Message }
                });
            }
        }

        /// <summary>
        /// Refresh your bearer token
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Refresh")]
        [HttpPost("refresh")]
        public IActionResult RefreshAuthentication()
        {
            string userId = User.Identity.Name;
            var user = _userService.GetById(userId);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                SecurityToken accessToken = tokenHandler.CreateToken(tokenDescriptor);

                var tokenString = tokenHandler.WriteToken(accessToken);

                return Ok(new ResultData()
                {
                    data = new
                    {
                        access_token = tokenString,
                        token_type = "bearer",
                        expires_in = "3600"
                    },
                    error = new Error() { errorCode = (int)ErrorCode.NoError, message = "" }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResultData()
                {
                    data = new { },
                    error = new Error() { errorCode = (int)ErrorCode.AuthenticationGeneric, message = ex.Message }
                });
            }

        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>All users</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var users = _userService.GetAll();
                return Ok(new ResultData()
                {
                    data = users,
                    error = new Error() { errorCode = (int)ErrorCode.NoError, message = "" }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResultData()
                {
                    data = new { },
                    error = new Error() { errorCode = (int)ErrorCode.UserGetAll, message = ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets the specified user
        /// </summary>
        /// <returns>The user</returns>
        /// <param name="id">Id of the user</param>
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                var user = _userService.GetById(id);
                return Ok(new ResultData()
                {
                    data = user,
                    error = new Error() { errorCode = (int)ErrorCode.NoError, message = "" }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResultData()
                {
                    data = new { },
                    error = new Error() { errorCode = (int)ErrorCode.UserGet, message = ex.Message }
                });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public IActionResult Update([FromBody]User user)
        {
            try
            {
                // save 
                var updated = _userService.Update(user);
                return Ok(new ResultData()
                {
                    data = updated,
                    error = new Error() { errorCode = (int)ErrorCode.NoError, message = "" }
                });
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new ResultData()
                {
                    data = new { },
                    error = new Error() { errorCode = (int)ErrorCode.UserUpdate, message = ex.Message }
                });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _userService.Delete(id);
                return Ok(new ResultData()
                {
                    data = new { message = "User deleted successfully" },
                    error = new Error() { errorCode = (int)ErrorCode.NoError, message = "" }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResultData()
                {
                    data = new { },
                    error = new Error() { errorCode = (int)ErrorCode.UserDelete, message = ex.Message }
                });
            }
        }
    }
}
