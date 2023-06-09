﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SeetourAPI.DAL.DTO;
using SeetourAPI.Data.Claims;
using SeetourAPI.Data.Context;
using SeetourAPI.Data.Models.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Text;

namespace SeetourAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SeetourContext context;

        public UserManager<SeetourUser> Usermanger { get; }

        public UserController(UserManager<SeetourUser> _Usermanger, IConfiguration configuration, SeetourContext context)
        {
            Usermanger = _Usermanger;
            _configuration = configuration;
            this.context = context;
        }


        [HttpPost]
        [Route("CustomerRegistration")]
        public async Task<ActionResult<TokenDto>> Register(CustomerRegistrationDto registrationDto)
        {

            var UserToAdd = new SeetourUser()
            {
                UserName = registrationDto.UserName,
                ProfilePic = registrationDto.profilepic,
                SSN = registrationDto.SSN,
                FullName = registrationDto.FullName,
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber

            };

            var result = await Usermanger.CreateAsync(UserToAdd, registrationDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var customerToAdd = new Customer()
            {
                Id = UserToAdd.Id,
                IsBlocked = false,
                // add any other properties you want to set for the customer object here
            };
            context.Customers.Add(customerToAdd);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,UserToAdd.Id),
                new Claim(ClaimTypes.Role,UserToAdd.SecurityLevel="customer"),
                new Claim(ClaimType.Status, customerToAdd.IsBlocked ? "Blocked" : "Allowed")
            };
            await Usermanger.AddClaimsAsync(UserToAdd, claims);

            await context.SaveChangesAsync();
            return NoContent();

        }


        [HttpPost]
        [Route("TourGuideRegistration")]
        public async Task<ActionResult<TokenDto>> Register(TourGuideRegistrationDto registrationDto)
        {

            var UserToAdd = new SeetourUser()
            {
                UserName = registrationDto.UserName,
                ProfilePic = registrationDto.profilepic,
                SSN = registrationDto.SSN,
                FullName = registrationDto.FullName,
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber
            };
            var result = await Usermanger.CreateAsync(UserToAdd, registrationDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            var customerToAdd = new TourGuide()
            {
                Id = UserToAdd.Id,
                RecipientAccountNumberOrIBAN = registrationDto.RecipientAccountNumberOrIBAN,
                RecipientBankNameAndAddress = registrationDto.RecipientBankNameAndAddress,
                RecipientBankSwiftCode = registrationDto.RecipientBankSwiftCode,
                RecipientNameAndAddress = registrationDto.RecipientNameAndAddress,
                TaxRegistrationNumber = registrationDto.TaxRegistrationNumber,
                IDCardPhoto = registrationDto.IDCardPhoto,
                // add any other properties you want to set for the customer object here
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,UserToAdd.Id),
                new Claim(ClaimTypes.Role,UserToAdd.SecurityLevel="TourGuide"),
                new Claim(ClaimType.Status, customerToAdd.Status.ToString())
            };

            await Usermanger.AddClaimsAsync(UserToAdd, claims);

            context.TourGuides.Add(customerToAdd);
            await context.SaveChangesAsync();


            return NoContent();

        }



        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            var user = await Usermanger.FindByNameAsync(loginDto.username);
            if (user == null)
            {
                return NotFound();
            }
            var isAuthenticated = await Usermanger.CheckPasswordAsync(user, loginDto.password);
            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            // Generate Secret Key Object
            var secretKeyString = _configuration.GetValue<string>("SecretKey") ?? string.Empty;
            var secretKeyInBytes = Encoding.ASCII.GetBytes(secretKeyString);
            var secretKey = new SymmetricSecurityKey(secretKeyInBytes);

            // Combination SecretKey, HashingAlgorithm
            var siginingCreedentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature);

            var expiry = DateTime.Now.AddDays(2);

            var claimlist = await Usermanger.GetClaimsAsync(user);

            var token = new JwtSecurityToken(
                claims: claimlist,
                expires: expiry,
                signingCredentials: siginingCreedentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new TokenDto(tokenString, expiry, user.SecurityLevel));
        }


    }

}