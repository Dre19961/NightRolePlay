using System.Collections.Generic;
using System.Linq;

namespace Lite.VehicleHandlers
{
    public static class VehiclesName
    {
        //реальные названия авто
        public static Dictionary<string, string> ModelList = new Dictionary<string, string>()
        {

              {"18velar", "Range Rover Velar" },
              {"63gls", "Mercedes-Benz GLS" },
              {"63gls2", "Mercedes-Benz GLS 63" },
              {"350z", "Nissan 350z" },
              {"2019m5", "BMW M5 Competition" },
              {"a45", "Mercedes-Benz A45" },
              {"agerars", "Koenigsegg Agera RS" },
              {"amggt16", "Mercedes-Benz AMG GT" },
              {"astondb11", "Aston Martin DB" },
              {"avtr", "Mercedes-Benz AVTR" },
              {"bentayga", "Bentley Bentayga" },
              {"bentley20", "Bentley Continental GT" },
              {"bmw730", "BMW 7 series" },
              {"bmwe70", "BMW X5 E70" },
              {"bmwg05", "BMW X5" },
              {"bmwg07", "BMW X7" },
              {"bmwm2", "BMW M2" },
              {"bmwm4", "BMW M4" },
              {"divo", "Bugatti Divo"},
              {"camry70", "Toyota Camry" },
              {"chiron19", "Bugatti Chiron" },
              {"cls63s", "Mercedes-Benz CLS 63s" },
              {"cullinan", "Rolls-Royce Cullinan" },
              {"cyber", "Tesla Cybertruck" },
              {"e63s", "Mercedes-Benz E 63s" },
              {"eqg", "Mercedes-Benz EQG" },
              {"g63amg6x6", "Mercedes-Benz G63 6x6" },
              {"ghost", "Rolls-Royce Ghost" },
              {"gle63", "Mercedes-Benz GLE 63" },
              {"gt63s", "Mercedes-Benz AMG GT 4-door" },
              {"huracan", "Lamborgini Huracan" },
              {"i8", "BMW I8" },
              {"jesko20", "Koenigsegg Jesko" },
              {"lc200", "Toyota Land Cruiser 200" },
              {"lex570", "Lexus LX" },
              {"mark2", "Toyota Mark 2" },
              {"modelx", "Tesla Model X" },
              {"panamera17turbo", "Porshe Panamera" },
              {"rs6", "Audi RS6" },
              {"rs72", "Audi RS7" },
              {"s63cab", "Mercedes-Benz S63 coupe" },
              {"s600", "Mercedes-Benz S600" },
              {"skyline", "Nissan Skyline" },
              {"svj63", "Lamborgini SVJ" },
              {"taycan21", "Porshe Taycan" },
              {"teslaroad", "Tesla Roadster" },
              {"urus", "Lamborgini Urus" },
              {"vclass", "Mercedes-Benz V class" },
              //modelname  //Realname
        };

        public static string GetRealVehicleName(string model)
        {
            if (ModelList.ContainsKey(model))
            {
                return ModelList[model];
            }
            else
            {
                return model;
            }
        }

        public static string GetVehicleModelName(string name)
        {
            if (ModelList.ContainsValue(name))
            {
                return ModelList.FirstOrDefault(x => x.Value == name).Key;
            }
            else
            {
                return name;
            }
        }
    }
}
