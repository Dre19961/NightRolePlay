using System.Collections.Generic;
using GTANetworkAPI;

namespace Lite.Fractions
{
    class Mafia : Script
    {
        public static Dictionary<int, Vector3> EnterPoints = new Dictionary<int, Vector3>()
        {
            { 10, new Vector3() },
            { 11, new Vector3() },
            { 12, new Vector3() },
            { 13, new Vector3() },
        };
        public static Dictionary<int, Vector3> ExitPoints = new Dictionary<int, Vector3>()
        {
            { 10, new Vector3() },
            { 11, new Vector3() },
            { 12, new Vector3() },
            { 13, new Vector3() },
        };

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            NAPI.TextLabel.CreateTextLabel("~r~Руслан Антонов", new Vector3(), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            NAPI.TextLabel.CreateTextLabel("~r~Алмаст Измалов", new Vector3(-1811.368, 438.4105, 129.7074), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            NAPI.TextLabel.CreateTextLabel("~r~Рамон Гертруда", new Vector3(-1549.287, -89.35114, 55.92917), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            NAPI.TextLabel.CreateTextLabel("~r~Бьянки Эспозито", new Vector3(1392.098, 1155.892, 115.4433), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            
            foreach (var point in EnterPoints)
            {
                NAPI.Marker.CreateMarker(1, point.Value - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(129, 159, 235), false, NAPI.GlobalDimension);

                var col = NAPI.ColShape.CreateCylinderColShape(point.Value, 1.2f, 2, NAPI.GlobalDimension);
                col.SetData("FRAC", point.Key);

                col.OnEntityEnterColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("FRACTIONCHECK", s.GetData<object>("FRAC"));
                    e.SetData("INTERACTIONCHECK", 64);
                };
                col.OnEntityExitColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("INTERACTIONCHECK", -1);
                };
            }

            foreach (var point in ExitPoints)
            {
                NAPI.Marker.CreateMarker(1, point.Value - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(129, 159, 235), false, NAPI.GlobalDimension);

                var col = NAPI.ColShape.CreateCylinderColShape(point.Value, 1.2f, 2, NAPI.GlobalDimension);
                col.SetData("FRAC", point.Key);

                col.OnEntityEnterColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("FRACTIONCHECK", s.GetData<object>("FRAC"));
                    e.SetData("INTERACTIONCHECK", 65);
                };
                col.OnEntityExitColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("INTERACTIONCHECK", -1);
                };
            }
        }
    }
}
