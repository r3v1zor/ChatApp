using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using ChatApp.Database;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Distributed;

namespace ChatApp.Controllers
{
    public class PerformanceController : Controller
    {
        private AppDbContext _ctx;
        private IDistributedCache _cache;

        public PerformanceController(AppDbContext ctx, IDistributedCache cache)
        {
            _ctx = ctx;
            _cache = cache;
        }
        
        [HttpGet]
        public IActionResult Index() => View(new Status {PostgresTime = 0.0, RedisTime = 0.0});

        [HttpPost("[action]")]
        public async Task<IActionResult> CalculatePostgres(int userId)
        {
            var start = DateTime.Now;
            await _ctx.TestUsers.FirstAsync(user => user.Id == userId);

            var postgresTime = (DateTime.Now - start).TotalMilliseconds;
            
            start = DateTime.Now;

            var userBytes = await _cache.GetAsync(userId.ToString());

            var redisTime = (DateTime.Now - start).TotalMilliseconds;
            
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(userBytes, 0, userBytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var user = (TestUser) new BinaryFormatter().Deserialize(memoryStream);
            }


            return RedirectToAction("Index", new Status {PostgresTime = postgresTime, RedisTime = redisTime});
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GenerateUsers(int count)
        {
            var testUsers = new List<TestUser>();
            for (int i = 0; i < count; i++)
            {
                var testUser = new TestUser {UserName = "Slave-{i}"};
                testUsers.Add(testUser);
                await _cache.SetAsync(i.ToString(), ObjectToByteArray(testUser));
            }

            await _ctx.TestUsers.AddRangeAsync(testUsers);
            
            await _ctx.SaveChangesAsync();

            return RedirectToAction("Index", new Status());
        }

        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        
        public class Status
        {
            public double PostgresTime { get; set; }
            public double RedisTime { get; set; }
        }
    }
}