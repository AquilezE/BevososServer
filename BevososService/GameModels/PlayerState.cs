using BevososService.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.GameModels
{
    public class PlayerState
    {
        public UserDto User { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public List<Monster> Monsters { get; set; } = new List<Monster>();
        public int ActionsPerTurn { get; set; } = 2;
        public bool Disconnected { get; set; } = false;

    }
}
