using System;
using System.Threading.Tasks;
using ChatApp.Database;
using ChatApp.Hubs;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private IHubContext<ChatHub> _chat;

        public ChatController(IHubContext<ChatHub> chat)
        {
            _chat = chat;
        }

        [HttpPost("[action]/{connectionId}/{roomName}")]
        public async Task<IActionResult> JoinChat(string connectionId, string roomName)
        {
            await _chat.Groups.AddToGroupAsync(connectionId, roomName);
            return Ok();
        }
        
        [HttpPost("[action]/{connectionId}/{roomName}")]
        public async Task<IActionResult> LeaveChat(string connectionId, string roomName)
        {
            await _chat.Groups.RemoveFromGroupAsync(connectionId, roomName);
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage(string text, int chatId, string roomName, [FromServices] AppDbContext ctx)
        {
            var message = new Message
            {
                ChatId = chatId,
                Text = text,
                AuthorName = User.Identity.Name,
                Timestamp = DateTime.Now
            };

            ctx.Messages.Add(message);
            
            await ctx.SaveChangesAsync();

            await _chat.Clients
                .Group(roomName)
                .SendAsync("ReceiveMessage", new
                {
                    Text = message.Text,
                    AuthorName = message.AuthorName,
                    ChatId = message.ChatId,
                    Timestamp = message.Timestamp.ToString("dd/MM/yyyy hh:mm:ss")
                });
            return Ok();
        }
    }
}