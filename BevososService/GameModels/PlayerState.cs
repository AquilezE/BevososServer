using BevososService.DTOs;
using System.Collections.Generic;

namespace BevososService.GameModels
{
    public class PlayerState
    {
        public UserDTO User { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public List<Monster> Monsters { get; set; } = new List<Monster>();
        public int ActionsPerTurn { get; set; } = 2;
        public bool Disconnected { get; set; } = false;
    }
}