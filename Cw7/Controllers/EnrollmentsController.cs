using Cw7.DAL;
using Cw7.DTOs.Requests;
using Cw7.DTOs.Responses;
using Cw7.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Cw7.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentDbService _dbService;

        public EnrollmentsController(IStudentDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{idEnrollment}")]
        public IActionResult GetEnrollment(int idEnrollment)
        {
            var enrollment = _dbService.GetEnrollment(idEnrollment);
            if (enrollment != null) {
                return Ok(new GetEntrollmentResponse
                {
                    IdEnrollment = enrollment.IdEnrollment,
                    IdStudy = enrollment.IdStudy,
                    Semester = enrollment.Semester,
                    StartDate = enrollment.StartDate
                });
            }
            
            return NotFound("Enrollment not found");
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            try
            {
                var enrollment = _dbService.CreateStudentEnrollment(
                    request.IndexNumber, request.FirstName, request.LastName,
                    DateTime.ParseExact(request.BirthDate, "dd.MM.yyyy", null), request.Studies);
                if (enrollment != null)
                {
                    return CreatedAtAction(nameof(GetEnrollment),
                        new { idEnrollment = enrollment.IdEnrollment },
                        new GetEntrollmentResponse
                        {
                            IdEnrollment = enrollment.IdEnrollment,
                            IdStudy = enrollment.IdStudy,
                            Semester = enrollment.Semester,
                            StartDate = enrollment.StartDate
                        });
                }
                
                return StatusCode(500, "Failed to process request");
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (FormatException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            var studies = _dbService.GetStudies(request.Studies);
            if (studies == null) {
                return NotFound("Studies not found");
            }

            var enrollment = _dbService.GetEnrollment(studies.IdStudy, request.Semester);
            if (enrollment == null) {
                return NotFound("Enrollment for semester dosen not exits");
            }

            var newEnrollment = _dbService.SemesterPromote(studies.IdStudy, request.Semester);
            return CreatedAtAction(nameof(GetEnrollment),
                new { idEnrollment = enrollment.IdEnrollment },
                new GetEntrollmentResponse
                {
                    IdEnrollment = newEnrollment.IdEnrollment,
                    IdStudy = newEnrollment.IdStudy,
                    Semester = newEnrollment.Semester,
                    StartDate = newEnrollment.StartDate
                });
        }
    }
}