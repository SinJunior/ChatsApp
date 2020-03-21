﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerChatsApp.Model
{
    public class Message
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public int UserId { get; set; }
        public string ChatroomName { get; set; }
        public ChatObject.ChatTypeMess ChatType { get; internal set; }
    }
}
