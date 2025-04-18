using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JogoDaVelha.Enums;

namespace JogoDaVelha.Classes
{
    public class GameResult
    {
        public Player Ganhador { get; set; } 
        public WinInfo Info { get; set; }
    }
}
