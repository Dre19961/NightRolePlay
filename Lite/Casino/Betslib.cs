using GTANetworkAPI;
using Newtonsoft.Json;
using Lite.Core;
using Lite.Fractions;
using LiteSDK;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lite.Casino
{
    class betlib : Script
    {
        private static nLog Log = new nLog("Betlib");
        public static Dictionary<int, Bet> Bets = new Dictionary<int, Bet>()
        {
            {1, new Bet(8,"Черное")},
            {2, new Bet(19,"Красное")},
            {3, new Bet(31,"Черное")},
            {4, new Bet(18,"Красное")},
            {5, new Bet(6,"Черное")},
            {6, new Bet(21,"Красное")},
            {7, new Bet(33,"Черное")},
            {8, new Bet(16,"Красное")},
            {9, new Bet(4,"Черное")},
            {10, new Bet(23,"Красное")},
            {11, new Bet(35,"Черное")},
            {12, new Bet(14,"Красное")},
            {13, new Bet(2,"Черное")},
            {14, new Bet(0,"Зеленое")},
            {15, new Bet(28,"Черное")},
            {16, new Bet(9,"Красное")},
            {17, new Bet(26,"Черное")},
            {18, new Bet(30,"Красное")},
            {19, new Bet(11,"Черное")},
            {20, new Bet(7,"Красное")},
            {21, new Bet(20,"Черное")},
            {22, new Bet(32,"Красное")},
            {23, new Bet(17,"Черное")},
            {24, new Bet(5,"Красное")},
            {25, new Bet(22,"Черное")},
            {26, new Bet(34,"Красное")},
            {27, new Bet(15,"Черное")},
            {28, new Bet(3,"Красное")},
            {29, new Bet(24,"Черное")},
            {30, new Bet(36,"Красное")},
            {31, new Bet(13,"Черное")},
            {32, new Bet(1,"Красное")},
            {33, new Bet(1000,"Зеленое")},
            {34, new Bet(27,"Красное")},
            {35, new Bet(10,"Черное")},
            {36, new Bet(25,"Красное")},
            {37, new Bet(29,"Черное")},
            {38, new Bet(12,"Красное")},
        };
         public class Bet
        {
            public int Number { get; }
            public string Color { get; }
            public Bet(int number, string color)
            {
                Number = number;
                Color = color;
            }
        }
    }
}