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
    class tablelib : Script
    {
        private static nLog Log = new nLog("Betlib");
        public static Dictionary<int, Tables> Casino = new Dictionary<int, Tables>()
        {
            { 0, new Tables(new Vector3(1144.762, 269.2798, -52.6358),
                new List<Vector3>(){
                    new Vector3(1143.725, 268.0235, -51.88085),
                    new Vector3(1145.645, 267.8848, -51.88085),
                    new Vector3(1146.205, 268.6835, -51.88085),
                    new Vector3(1145.695, 270.3235, -51.88085),
            },true,true,true,0)},
            { 1, new Tables(new Vector3(1151.271, 263.0273, -52.6358),
                new List<Vector3>(){
                    new Vector3(1150.531, 261.8609, -51.88085),
                    new Vector3(1152.451, 261.6934, -51.88085),
                    new Vector3(1153.011, 262.5209, -51.88085),
                    new Vector3(1152.501, 264.1609, -51.88085),
            },true,true,true,0)},
            { 2, new Tables(new Vector3(1148.961, 248.513, -52.6358),
                new List<Vector3>(){
                    new Vector3(1148.216, 247.3489, -51.08075),
                    new Vector3(1150.136, 247.0892, -51.08075),
                    new Vector3(1150.696, 248.0089, -51.08075),
                    new Vector3(1150.186, 249.6489, -51.08075),
            },true,true,true,0)},
            { 3, new Tables(new Vector3(1143.681, 251.2601, -52.6358),
                new List<Vector3>(){
                    new Vector3(1142.978, 250.0813, -51.08075),
                    new Vector3(1144.898, 249.8888, -51.08075),
                    new Vector3(1145.458, 250.7413, -51.08075),
                    new Vector3(1144.948, 252.3813, -51.08075),
            },true,true,true,0)},
            { 4, new Tables(new Vector3(1133.354, 262.2992, -52.6358),
                new List<Vector3>(){
                    new Vector3(1132.48, 261.1116, -51.08075),
                    new Vector3(1134.4, 260.9421, -51.08075),
                    new Vector3(1134.96, 261.7716, -51.08075),
                    new Vector3(1134.45, 263.4116, -51.08075),
            },true,true,true,0)},
            { 5, new Tables(new Vector3(1130.1788, 266.77032, -52.135723),
                new List<Vector3>(){
                    new Vector3(1129.298, 265.657, -51.08075),
                    new Vector3(1131.218, 265.5161, -51.08075),
                    new Vector3(1131.778, 266.317, -51.08075),
                    new Vector3(1131.268, 267.9569, -51.08075),
            },true,true,true,0)},
        };

        public class Tables
        {
            public Vector3 TablePosition { get; }
            public List<Vector3> SeatsPositions { get; }
            public List<bool> Seats { get; set; } = new List<bool> { true, true, true, true };
            public bool Tablestartgame { get; set; }
            public bool Firstbet { get; set; }
            public bool StartTimer { get; set; }
            public int Ballends { get; set; }


            public Tables(Vector3 position, List<Vector3> seatsPositions, bool tablestartgame, bool firstbet, bool starttimer, int ballends)
            {
                TablePosition = position;
                SeatsPositions = seatsPositions;
                Tablestartgame = tablestartgame;
                Firstbet = firstbet;
                StartTimer = starttimer;
                Ballends = ballends;
            }
        }
    }
}