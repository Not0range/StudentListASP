using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudentList.Models;

namespace StudentList.Controllers
{
    [ApiController]
    [Route("")]
    public class AuthorizationController : ControllerBase
    {
        const string secret = "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdC4=";
        private readonly IHttpClientFactory _clientFactory;
        public static List<Student> students = new List<Student>();

        public AuthorizationController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet("/login")]
        public async Task<ActionResult<string>> CheckLogin()
        {
            if(!Request.Cookies.ContainsKey("id"))
                return StatusCode(401);

            var body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "Refresh table Cookies"}), 
                Encoding.UTF8, "application/json");
            var client = _clientFactory.CreateClient("sql");
            (await client.PostAsync("", body)).EnsureSuccessStatusCode();

            body = new StringContent(
                JsonSerializer.Serialize(new{stmt =  
                "select Cookies.ID, Cookies.Cookie, Logins.login " +
                "from Cookies, Logins Where Cookies.Cookie = '" + 
                Request.Cookies["id"] + "' and Cookies.ID = Logins.ID"}), 
                Encoding.UTF8, "application/json");
            var response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            var sql = JsonSerializer.Deserialize<SqlResponse>(await response.Content.ReadAsStringAsync());
            if(sql.rowcount > 0)
                return sql.rows[0][2].ToString();
            else
                return Unauthorized();
        }

        [HttpPost("/login")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Login([FromForm]IDictionary<string, string> form)
        {
            if(!form.ContainsKey("login") || !form.ContainsKey("password"))
                return BadRequest();

            var body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "select * from Logins " +
                "Where login = '" + form["login"].ToLower() + 
                "' and password = '" + form["password"] + "'"}), 
                Encoding.UTF8, "application/json");
            var client = _clientFactory.CreateClient("sql");
            var response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            var sql = JsonSerializer.Deserialize<SqlResponse>(await response.Content.ReadAsStringAsync());
            if(sql.rowcount == 0)
                return Forbid();
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("login", form["login"])
                }),
                
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Convert.FromBase64String(secret)), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            string key = new JwtSecurityTokenHandler().CreateEncodedJwt(tokenDescriptor);
            int id;
            ((JsonElement)sql.rows[0][0]).TryGetInt32(out id);

            body = new StringContent(
                JsonSerializer.Serialize(new {stmt = 
                "select * from Cookies " +
                "Where cookie = '" + key + "'"}), 
                Encoding.UTF8, "application/json");
            response = await client.PostAsync("", body);
            response.EnsureSuccessStatusCode();
            sql = JsonSerializer.Deserialize<SqlResponse>(await response.Content.ReadAsStringAsync());
            if(sql.rowcount == 0)
            {
                var str = "insert into Cookies values(" + id + ", '" + key + "')";
                body = new StringContent(
                    JsonSerializer.Serialize(new {stmt = 
                    "insert into Cookies values(" + id + ", '" + key + "')"}), 
                    Encoding.UTF8, "application/json");
                response = await client.PostAsync("", body);
                response.EnsureSuccessStatusCode();
            }

            Response.Cookies.Append("id", key);
            return Redirect("/");
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            if(!Request.Cookies.ContainsKey("id"))
                return StatusCode(401);
            
            Response.Cookies.Delete("id");
            return Redirect("/");
        }
    }
}