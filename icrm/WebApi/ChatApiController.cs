﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using icrm.Models;
using icrm.RepositoryImpl;
using icrm.RepositoryInterface;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity.Owin;
using RabbitMQ.Client;

namespace icrm.WebApi
{
    using icrm.Events;

    [Authorize]
    public class ChatApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        private UserInterface userService;
        private ChatInterface chatService;
        private MessageInterface messageService;
        private ChatRequestInterface chatRequestService;
        private EventService eventService;

        public ChatApiController()
        {
            userService = new UserRepository();
            chatService = new ChatRepository();
            messageService = new MessageRepository();
            chatRequestService = new ChatRequestRepository();
            this.eventService = new EventService();
        }

        [HttpPost]
        [Route("api/chat/send")]
        public IHttpActionResult sendmsg([FromBody]Message message)
        {
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            ApplicationUser sender = user.Result;
            message.SenderId = sender.Id;
            ApplicationUser reciever;
            if (string.IsNullOrEmpty(message.RecieverId) && this.chatService.IsActive(message.ChatId))
            {
                Debug.Print(message.RecieverId);
            }
            Debug.Print(message.ChatId + "--------chat id by mudassir-----" + message.Text);
            if (message.ChatId == 0 || !this.chatService.IsActive(message.ChatId))
            {
                reciever = ProcessMessage(message, sender);
                Debug.Print("it shud be here-----");
                if (reciever == null)
                {
                    Debug.Print("process msg rerurned null----");
                    return Ok();

                }

            }
            else
            {
                reciever = userService.findUserOnId(message.RecieverId);
            }


            Producer producer = new Producer("messageexchange", ExchangeType.Direct);
            Message msgWithId = new Message();
            if (producer.ConnectToRabbitMQ())
            {
                msgWithId = SendChatMessage(message, sender, reciever);

                if (!chatService.IsActive(msgWithId.ChatId))
                    chatService.changeActiveStatus(msgWithId.ChatId, true);

                producer.send(msgWithId);
                this.eventService.NotifyHrAboutChat(msgWithId);
                this.eventService.changeToggle(reciever);
            }
            return Ok(msgWithId);
        }

        [HttpGet]
        [Route("api/chat/receive/{id}")]
        public IHttpActionResult receivemsg(int id)
        {
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            ApplicationUser receiver = user.Result;
            List<Message> messages = messageService.GetMessagesOnStatusAndChatId(Constants.RECEIVED, id, receiver.Id);
            
            return Ok(messages);
        }

        [HttpGet]
        [Route("api/chat/hr/available")]
        public string checkHrAvailability()
        {
            ApplicationUser reciever = userService.GetAllAvailableUsers(Constants.ROLE_HR).FirstOrDefault();
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            ApplicationUser sender = user.Result;
            if (this.chatRequestService.ChatRequestsSize() > 0)
            {
                if (!chatRequestService.CheckRequestExistsOfUser(sender.Id))
                {
                    ChatRequest chatRequest = new ChatRequest();
                    chatRequest.UserId = sender.Id;
                    chatRequestService.Save(chatRequest);
                }

                return "false";
            }
            else
            {
                if (reciever == null)
                {
                    if (!chatRequestService.CheckRequestExistsOfUser(sender.Id))
                    {
                        ChatRequest chatRequest = new ChatRequest();
                        chatRequest.UserId = sender.Id;
                        chatRequestService.Save(chatRequest);
                    }

                    return "false";

                }

            }
            startConsumer(sender.UserName);
            return "true";
        }

        [HttpPost]
        [Route("api/chat/close")]
        public IHttpActionResult closeChat([FromBody]Message message)
        {
            //tell mudassir to send message with chatid and recieverid
            Debug.Print(message.Text + "-----user closes chat----" + message.RecieverId);
            ApplicationUser reciever = userService.findUserOnId(message.RecieverId);
            chatService.closeAllActiveChatsOfUser(message.RecieverId);
            message.Reciever = reciever;
            if (reciever != null)
            {
                reciever.available = true;
                userService.Update(reciever);
                this.chatService.changeActiveStatus(message.ChatId, false);
                this.eventService.chatClosedByUser(message);
            }

            Debug.Print("----close chat window-----"+User.Identity.Name);
            Consumer consumer = (Consumer)HttpContext.Current.Application[User.Identity.Name];
            Debug.Print(consumer+"-------consukmer her for despose");
            consumer?.Dispose();
            return Ok();
        }

        [HttpGet]
        [Route("api/chat/window/open")]
        public IHttpActionResult chatWindowOpen()
        {
            Debug.Print("chat window opened----------");
            var Name1 = User.Identity.Name;
            Task<ApplicationUser> user = UserManager.FindByNameAsync(Name1);
            ApplicationUser sender = user.Result;
            Chat chat =chatService.hasUserChatActive(sender.Id);
            var map = new
            {
                status = chat!=null?true:false,
                recieverId = chat?.UserOneId == sender.Id?chat?.UserTwoId:chat?.UserOneId,
                chatId=chat?.Id,
                messages = messageService.GetMessagesOfUser(sender.Id)
            };
            startConsumer(sender.UserName);

            return Ok(map);
        }



        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationUser ProcessMessage(Message message, ApplicationUser sender)
        {
            if (this.chatRequestService.ChatRequestsSize() > 0)
            {
                if (this.chatRequestService.CheckRequestExistsOfUser(message.SenderId))
                {
                    message.ChatId = null;
                    message.RecieverId = null;
                    message.SentTime = DateTime.Now;
                    this.messageService.Save(message);
                    return null;
                }
                else
                {
                    ChatRequest chatRequest = new ChatRequest();
                    chatRequest.UserId = message.SenderId;
                    chatRequestService.Save(chatRequest);
                    message.ChatId = null;
                    message.RecieverId = null;
                    message.SentTime = DateTime.Now;
                    this.messageService.Save(message);
                    return null;
                }
            }
            else
            {
                ApplicationUser reciever = userService.GetAllAvailableUsers(Constants.ROLE_HR).FirstOrDefault();

                //Debug.Print(reciever.UserName+"-----username-----"+reciever.Id);
                if (reciever != null && (this.chatService.IsActive(message.ChatId) || message.ChatId == 0))
                {

                    reciever.available = false;
                    userService.Update(reciever);
                    return reciever;
                }
                else
                {
                    ChatRequest chatRequest = new ChatRequest();
                    chatRequest.UserId = message.SenderId;
                    chatRequestService.Save(chatRequest);
                    message.ChatId = null;
                    message.RecieverId = null;
                    message.SentTime = DateTime.Now;
                    this.messageService.Save(message);
                    return null;
                }
            }

            return null;
        }

        public Message SendChatMessage(Message message, ApplicationUser sender, ApplicationUser reciever)
        {
            int? chatId2 = chatService.getChatIdOfUsers(sender.Id, reciever.Id);
            Debug.Print("Chat id is  " + chatId2);
            if (chatId2 == null)
            {
                Debug.Print("Chat id 2 is null but how--" + chatId2);
                Chat chat = new Chat();
                chat.UserOneId = sender.Id;
                chat.UserTwoId = reciever.Id;
                chat.active = true;
                Debug.Print("chat details----" + chat.ToString());
                chatId2 = chatService.Save(chat);
            }



            message.SenderId = sender.Id;
            if (message.RecieverId.IsNullOrWhiteSpace())
            {
                message.RecieverId = reciever.Id;
            }

            message.SentTime = DateTime.Now;
            message.ChatId = chatId2;
            Message msgWithId = messageService.Save(message);
            return msgWithId;
        }

        public void startConsumer(string username)
        {
            Consumer consumer = new Consumer("messageexchange", ExchangeType.Direct);
            HttpContext.Current.Application[username] = consumer;
            if (consumer.ConnectToRabbitMQ())
                consumer.StartConsuming(username);

        }
    }

}
