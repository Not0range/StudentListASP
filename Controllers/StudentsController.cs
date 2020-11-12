using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentList.Models;

namespace StudentList.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        public static List<Student> students = new List<Student>();
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(ILogger<StudentsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Student> Get()
        {
            return students;
        }

        [HttpGet("last-id")]
        public int GetLastId()
        {
            return students.Count > 0 ? students[^1].id + 1 : 0;
        }

        [HttpPut]
        [Consumes("application/json")]
        public IActionResult Add(Student s)
        {
            students.Add(s);
            return Created("/student", s);
        }

        [HttpPost]
        public IActionResult Edit(Student s)
        {
            Student temp = students.Find(student => student.id == s.id);
            temp.fio = s.fio;
            temp.course = s.course;
            temp.spec = s.spec;
            temp.number = s.number;
            return Accepted();
        }

        [HttpDelete]
        public IActionResult Delete(IEnumerable<int> m)
        {
            List<Student> temp = students.FindAll(s => m.Any(i => i == s.id));
            foreach(var t in temp)
                students.Remove(t);
            return Accepted();
        }
    }
}