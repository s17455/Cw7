using Cw7.DAL;
using Cw7.DTOs.Requests;
using Cw7.DTOs.Responses;
using Cw7.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cw7.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentDbService _dbService;
        private readonly IConfiguration _configuration;

        public StudentsController(IStudentDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequest request)
        {
            var student = _dbService.GetStudent(request.Username);
            if (student == null)
                return NotFound(new ErrorResponse { Message = "User does not exists" });

            static string CreateHash(string password, string salt)
            {
                return Convert.ToBase64String(
                    KeyDerivation.Pbkdf2(
                        password: password,
                        salt: Encoding.UTF8.GetBytes(salt),
                        prf: KeyDerivationPrf.HMACSHA512,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8
                    )
                );
            }

            if (CreateHash(request.Password, student.Salt) != student.Password) {
                return Unauthorized(new ErrorResponse
                {
                    Message = "Wrong password"
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, student.IndexNumber),
                new Claim(ClaimTypes.Name, student.FirstName + "_" + student.LastName),
                new Claim(ClaimTypes.Role, "student")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "s17455",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            var response = new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = Guid.NewGuid().ToString()
            };

            var tokenRefreshingResult = _dbService.CreateRefreshToken(new RefreshToken { Id = response.RefreshToken, IndexNumber = student.IndexNumber });

            if (tokenRefreshingResult < 1) {
                return Unauthorized();
            }

            return Ok(response);
        }

        [HttpPost("refresh-token/{refreshToken}")]
        [AllowAnonymous]
        public IActionResult RefreshToken(string refreshToken)
        {
            var student = _dbService.GetRefreshTokenOwner(refreshToken);
            if (student == null) {
                return NotFound(new ErrorResponse
                {
                    Message = "Refresh roken dosen't exists or is incorrect"
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, student.IndexNumber),
                new Claim(ClaimTypes.Name, student.FirstName + "_" + student.LastName),
                new Claim(ClaimTypes.Role, "student")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "s17455",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );
            var response = new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = Guid.NewGuid().ToString()
            };

            int refreshTokenCreationResult = _dbService.CreateRefreshToken(new RefreshToken { Id = response.RefreshToken, IndexNumber = student.IndexNumber });
            int oldRefreshTokenRemovalResult = _dbService.DeleteRefreshToken(refreshToken);

            if (refreshTokenCreationResult == 0 || oldRefreshTokenRemovalResult == 0) {
                return Unauthorized("Error while refreshing token");
            }

            return Ok("Token refreshed successfully");
        }

        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            return Ok(_dbService.GetStudents(orderBy));
        }

        [HttpGet("{indexNumber}")]
        public IActionResult GetStudent(string indexNumber)
        {
            var student = _dbService.GetStudent(indexNumber);
            if (student != null)
                return Ok(student);
            else
                return NotFound("Student with index " + indexNumber + " does not exists.");
        }

        [HttpGet("{indexNumber}/enrollment")]
        public IActionResult GetStudentEnrollment(string indexNumber)
        {
            var student = _dbService.GetStudentEnrollment(indexNumber);
            
            if (student != null) {
                return Ok(student);
            }

            return NotFound("Student with index " + indexNumber + " does not exists.");
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            if (_dbService.CreateStudent(student) > 0){
                return Ok(student);
            }

            return Conflict(student);
        }

        [HttpPut("{indexNumber}")]
        public IActionResult UpdateStudent(string indexNumber, Student student)
        {
            if (_dbService.UpdateStudent(indexNumber, student) > 0) {
                return Ok("Update finished");
            }

            return NotFound("Student with index " + indexNumber + " does not exists.");
        }

        [HttpDelete("{indexNumber}")]
        public IActionResult DeleteStudent(string indexNumber)
        {
            if (_dbService.DeleteStudent(indexNumber) > 0) {
                return Ok("Deleting done");
            }

            return NotFound("Student with index " + indexNumber + " does not exists.");
        }
    }
}