﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerChatsApp.Model
{
    public class Chatroom
    {
        public int Id { get; set; }
        public int ChatTypeId { get; set; }
        public string RoomName { get; set; }

        public ChatType ChatType { get; set; }
        public ICollection<Chat> Chats { get; set; }
    }
}
