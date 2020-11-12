using System;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Net.Http;
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
        private readonly IHttpClientFactory _clientFactory;
        public static List<Student> students = new List<Student>();
        public StudentsController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public async Task<object[][]> Get()
        {
            var body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "Select * from Students order by id"}), 
                Encoding.UTF8, "application/json");
            var client = _clientFactory.CreateClient("sql");
            var response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<SqlResponse>(await (response).Content.ReadAsStringAsync()).rows;
        }

        [HttpGet("last-id")]
        public async Task<int> GetLastId()
        {
            var body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "select ID from Students Order by ID desc Limit 1"}), 
                Encoding.UTF8, "application/json");
            var client = _clientFactory.CreateClient("sql");
            var response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();

            var sql = JsonSerializer.Deserialize<SqlResponse>(await response.Content.ReadAsStringAsync());
            int id = 0;
            if(sql.rowcount > 0)
            {
                ((JsonElement)sql.rows[0][0]).TryGetInt32(out id);
                id++;
            }
            return id;
        }

        [HttpPut]
        [Consumes("application/json")]
        public async Task<IActionResult> Add(Student s)
        {
            if(!Request.Cookies.ContainsKey("id"))
                return StatusCode(401);

            var body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "select * from Cookies " +
                "Where cookie = '" + Request.Cookies["id"] + "'"}), 
                Encoding.UTF8, "application/json");
            var client = _clientFactory.CreateClient("sql");
            var response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            if(JsonSerializer.Deserialize<SqlResponse>(await response.Content.ReadAsStringAsync()).rowcount == 0)
                return StatusCode(401);
            
            body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "insert into Students values(" +
                $"{s.id},'{s.fio}', {s.course}, '{s.spec}', '{s.number}')"}), 
                Encoding.UTF8, "application/json");
            response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            return Created("/student", s);
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Edit(Student s)
        {
            if(!Request.Cookies.ContainsKey("id"))
                return StatusCode(401);

            var body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "select * from Cookies " +
                "Where cookie = '" + Request.Cookies["id"] + "'"}), 
                Encoding.UTF8, "application/json");
            var client = _clientFactory.CreateClient("sql");
            var response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            if(JsonSerializer.Deserialize<SqlResponse>(await response.Content.ReadAsStringAsync()).rowcount == 0)
                return StatusCode(401);
            
            body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "update Students set " +
                $"fio='{s.fio}', course={s.course}, spec='{s.spec}', num='{s.number}'" +
                $"where id={s.id}"}), 
                Encoding.UTF8, "application/json");
            response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            return Accepted();
        }

        [HttpDelete]
        [Consumes("application/json")]
        public async Task<IActionResult> Delete(IEnumerable<int> m)
        {
            if(!Request.Cookies.ContainsKey("id"))
                return StatusCode(401);

            var body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "select * from Cookies " +
                "Where cookie = '" + Request.Cookies["id"] + "'"}), 
                Encoding.UTF8, "application/json");
            var client = _clientFactory.CreateClient("sql");
            var response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            if(JsonSerializer.Deserialize<SqlResponse>(await response.Content.ReadAsStringAsync()).rowcount == 0)
                return StatusCode(401);
            
            body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "delete from Students where " + string.Join(" or ", m.Select(i => "id=" + i))}), 
                Encoding.UTF8, "application/json");
            response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            return Accepted();
        }
    }
}