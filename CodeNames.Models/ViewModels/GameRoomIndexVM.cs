﻿namespace CodeNames.Models.ViewModels
{
    public class GameRoomIndexVM
    {
        public List<GameRoom> GameRooms { get; set; }
        public string PageTitle {  get; set; }
        public bool HasItems { get; set; } = false;
    }
}
