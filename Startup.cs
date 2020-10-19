using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace StudentList
{
    public class Startup
    {
        List<Student> students = new List<Student>();

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/html; Charset=\"UTF-8\"";
                    await context.Response.SendFileAsync("static/index.html");
                });

                endpoints.MapGet("/src/{filename}", async context =>
                {
                    await context.Response.SendFileAsync("static" + context.Request.Path);
                });

                endpoints.MapGet("/students", async context =>
                {
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(students.ToArray()));
                });

                endpoints.MapGet("/last-id", async context =>
                {
                    await context.Response.WriteAsync((students.Count > 0 ? students[students.Count - 1].id + 1 : 0).ToString());
                });

                endpoints.MapPut("/students", async context =>
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        await context.Request.Body.ReadAsync(buffer, 0, (int)context.Request.ContentLength);
                        students.Add(JsonConvert.DeserializeObject<Student>(Encoding.UTF8.GetString(buffer)));

                        context.Response.StatusCode = 201;
                        await context.Response.CompleteAsync();
                    }
                    catch(JsonException)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.CompleteAsync();
                    }
                });

                endpoints.MapPost("/students", async context =>
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        await context.Request.Body.ReadAsync(buffer, 0, (int)context.Request.ContentLength);
                        var s =  JsonConvert.DeserializeObject<Student>(Encoding.UTF8.GetString(buffer));

                        int i = 0;
                        for (; i < students.Count; i++)
                            if (students[i].id == s.id)
                                break;

                        students[i].fio = s.fio;
                        students[i].course = s.course;
                        students[i].spec = s.spec;
                        students[i].number = s.number;

                        context.Response.StatusCode = 202;
                        await context.Response.CompleteAsync();
                    }
                    catch (JsonException)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.CompleteAsync();
                    }
                });

                endpoints.MapDelete("/students", async context =>
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        await context.Request.Body.ReadAsync(buffer, 0, (int)context.Request.ContentLength);
                        var arr = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(buffer));

                        for (int i = 0; i < students.Count; i++)
                            if (Array.IndexOf(arr, students[i].id) != -1)
                                students.RemoveAt(i--);

                        context.Response.StatusCode = 202;
                        await context.Response.CompleteAsync();
                    }
                    catch (JsonException)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.CompleteAsync();
                    }
                });
            });
        }
    }

#pragma warning disable CS0649
    class Student
    {
        public int id;

        public string fio;

        public int course;

        public string spec;

        public string number;
    }
#pragma warning restore CS0649
}
