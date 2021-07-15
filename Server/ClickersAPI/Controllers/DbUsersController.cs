using AutoMapper;
using ClickersAPI.DTO;
using ClickersAPI.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ClickersAPI.Controllers
{
    [ApiController]
    [Route("api/dbusers")]
    public class DbUsersController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly ApplicationDbContext m_Context;
        private readonly UserManager<IdentityUser> m_UserManager;
        private readonly SignInManager<IdentityUser> m_SignInManager;
        private readonly IConfiguration m_Configuration;
        private readonly IMapper m_Mapper;

        public DbUsersController(
            ApplicationDbContext _Context,
            UserManager<IdentityUser> _UserManager,
            SignInManager<IdentityUser> _SignInManager,
            IConfiguration _Configuration,
            IMapper _Mapper)
        {
            m_Context = _Context;
            m_UserManager = _UserManager;
            m_SignInManager = _SignInManager;
            m_Configuration = _Configuration;
            m_Mapper = _Mapper;
        }

        [HttpGet("Users")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "vip_admin")]
        public async Task<ActionResult<List<UserDto>>> Get([FromBody] PaginationDto _PaginationDto)
        {
            var queryable = m_Context.Users.AsQueryable();
            queryable = queryable.OrderBy(_X => _X.Email);
            await HttpContext.InsertPaginationParametersResponse(queryable, _PaginationDto.RecordsPerPage);
            var users = await queryable.Paginate(_PaginationDto).ToListAsync();
            return m_Mapper.Map<List<UserDto>>(users);
        }

        [HttpGet("Roles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "vip_admin")]
        public async Task<ActionResult<List<string>>> GetRoles()
        {
            return await m_Context.Roles.Select(_X => _X.Name).ToListAsync();
        }

        [HttpPost("AssignRole")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "vip_admin")]
        public async Task<ActionResult> AssignRole([FromBody] EditRoleDto _EditRoleDto)
        {
            var user = await m_UserManager.FindByIdAsync(_EditRoleDto.UserId);
            if (user == null)
                return NotFound();

            await m_UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, _EditRoleDto.RoleName));
            return NoContent();
        }

        [HttpPost("RemoveRole")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "vip_admin")]
        public async Task<ActionResult> RemoveRole([FromBody] EditRoleDto _EditRoleDto)
        {
            var user = await m_UserManager.FindByIdAsync(_EditRoleDto.UserId);
            if (user == null)
                return NotFound();

            await m_UserManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, _EditRoleDto.RoleName));
            return NoContent();
        }

        [ProducesResponseType(400)]
        [HttpPost("Create", Name = "createUser")]
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo _Model)
        {
            var user = new IdentityUser { UserName = _Model.EmailAddress, Email = _Model.EmailAddress };
            var result = await m_UserManager.CreateAsync(user, _Model.Password);

            if (result.Succeeded) 
                return await BuildToken(_Model);
            
            return BadRequest(result.Errors);
        }

        [HttpPost("Login", Name = "Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo _Model)
        {
            var result = await m_SignInManager.PasswordSignInAsync(
                _Model.EmailAddress, _Model.Password, false, false);

            if (result.Succeeded) 
                return await BuildToken(_Model);
            
            return BadRequest("Invalid login attempt");
        }

        [HttpPost("RenewToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserToken>> Renew()
        {
            var userInfo = new UserInfo {EmailAddress = HttpContext.User.Identity.Name};

            return await BuildToken(userInfo);
        }


        private async Task<UserToken> BuildToken(UserInfo _UserInfo)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, _UserInfo.EmailAddress),
                new Claim(ClaimTypes.Email, _UserInfo.EmailAddress)
            };

            var identityUser = await m_UserManager.FindByEmailAsync(_UserInfo.EmailAddress);
            var claimsDb = await m_UserManager.GetClaimsAsync(identityUser);

            claims.AddRange(claimsDb);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(m_Configuration["JWT:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(1);
            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                expires: expiration,
                signingCredentials: creds
                );

            return new UserToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
