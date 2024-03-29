﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class GameOverMessage
    {
        public bool IsVictory { get; set; }
        public int Winner { get; set; }
        public bool GameOver { get; set; }

    }

    public static class MessageEncoder
    {
        public static string SerializarGameOverMensagem(GameOverMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }
    }

}
