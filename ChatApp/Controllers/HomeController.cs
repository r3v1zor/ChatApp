using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.Database;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private AppDbContext _ctx;

        public HomeController(ILogger<HomeController> logger, AppDbContext ctx)
        {
            _logger = logger;
            _ctx = ctx;
        }

        public IActionResult Index()
        {
            var chats = _ctx.Chats.ToList();
            return View(chats);
        }

        [HttpGet("{id}")]
        public IActionResult Chat(int id)
        {
            var chat = _ctx.Chats
                .Include(x => x.Messages)
                .FirstOrDefault(x => x.Id == id);
            return View(chat);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int chatId, string text)
        {
            var message = new Message
            {
                ChatId = chatId,
                Text = text,
                AuthorName = User.Identity.Name,
                Timestamp = DateTime.Now
            };

            _ctx.Messages.Add(message);
            await _ctx.SaveChangesAsync();

            return RedirectToAction("Chat", new {id = chatId});
        }
        
        
        
        [HttpPost]
        public async Task<IActionResult> CreateRoom(string roomName)
        {
            var chat = new Chat
            {
                Name = roomName
            };

            _ctx.Chats.Add(chat);
            await _ctx.SaveChangesAsync();

            Console.WriteLine(chat);
            
            chat.Users.Add(new ChatUser
            {
                ChatId = chat.Id,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserRole = UserRole.Member
            });
            // _ctx.Chats.Add(chat);

            await _ctx.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        public async Task<IActionResult> JoinChat(string chatId)
        {
            int.TryParse(chatId, out int id);
            var chatUser = new ChatUser
            {
                ChatId = id,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserRole = UserRole.Member
            };

            _ctx.ChatUsers.Add(chatUser);
            await _ctx.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}