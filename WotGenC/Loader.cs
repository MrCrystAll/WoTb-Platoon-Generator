using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;

namespace WotGenC
{
    public class Loader
    {

        public static void LoadBackupsFor(List<Player> players)
        {
            string[] names = new string[players.Count];
            uint stringCount = 0;

            foreach (var player in players)
            {
                if (!File.Exists($"Backups/{player.Id}.xml"))
                {
                    names[stringCount] = player.Pseudo;
                    stringCount++;
                    continue;
                }
                
                player.Tanks.Clear();
                using FileStream fs = new FileStream($"Backups/{player.Id}.xml",
                    FileMode.Open);

                using XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

                DataContractSerializer ser = new DataContractSerializer(typeof(ListOfTanks));
                var tank = (ListOfTanks)ser.ReadObject(reader);
                player.Tanks = tank;

                // using (StreamReader reader = new StreamReader($"Backups/{player.Id}.json"))
                // {
                //     var result = reader.ReadToEnd();
                //     var json = JObject.Parse(result);
                //
                //     uint i = 0;
                //     var token = json["data"].First;
                //
                //     while (token != null)
                //     {
                //         player.Tanks.Add(new Tank(
                //             (string)token.First["Id"], 
                //             (string)token.First["Name"], 
                //             IntToTier((int)token.First["Tier"]),
                //             (string)token.First["Image"]));
                //         
                //         token = token.Next;
                //     }
                //}
            }

            if (stringCount == 0) return;
            string message = "Backup files not found for the followings player : \n\n";
            for (int i = 0; i < stringCount; i++)
            { 
                message += $" - {names[i]}\n";
            }
            Debug.Write(message);

        }

        public static bool LoadTanks(string id, ListOfTanks tanks, Stream stream)
        {

            if (stream is null) return false;

            ListOfTanks list = tanks;

            using StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            try
            {
                RequestVerifyier.CheckResult(json);
                list.Clear();
                var p = JObject.Parse(json);

                int i = 0;

                while (i < p["data"][id].Count())
                {
                    string idt = (string)p["data"][id][i]["tank_id"];
                    using (StreamReader reader2 = new StreamReader("AllTanks.json"))
                    {
                        string json2 = reader2.ReadToEnd();
                        var p2 = JObject.Parse(json2);

                        if (p2["data"][idt] != null)
                        {
                            var nom = (string)p2["data"][idt]["name"];
                            var tier = (int)p2["data"][idt]["tier"];

                            TankType type = StringToType((string)p2["data"][idt]["type"]);

                            //Trace.WriteLine($"{idt} => {nom}");
                            list.Add(new Tank(idt, nom, IntToTier(tier),
                                (string)p2["data"][idt]["images"]["preview"], type));
                        }
                    }

                    i++;
                }

                list.Sort();

                Saver.SaveBackup($"Backups/{id}.xml", list);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't open stats :\n" +
                                $"{e.Message}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private static Tier IntToTier(int t)
        {
            return t switch
            {
                1 => Tier.I,
                2 => Tier.II,
                3 => Tier.III,
                4 => Tier.IV,
                5 => Tier.V,
                6 => Tier.VI,
                7 => Tier.VII,
                8 => Tier.VIII,
                9 => Tier.IX,
                10 => Tier.X,
                _ => Tier.X
            };
        }

        private static TankType StringToType(string s)
        {
            return s switch
            {
                "mediumTank" => TankType.MEDIUM,
                "heavyTank" => TankType.HEAVY,
                "AT-SPG" => TankType.TD,
                "lightTank" => TankType.LIGHT,
                _ => throw new ArgumentException("Wrong type value")
            };
        }

        public static List<Tank> FindTanksById(params string[] ids)
        {
            List<Tank> tanks = new List<Tank>();

            FileStream fs = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "AllTanks.json"), FileMode.OpenOrCreate, FileAccess.Read);
            using StreamReader reader = new StreamReader(fs);
            string text = reader.ReadToEnd();
            JObject all = JObject.Parse(text);
                
            foreach (var id in ids)
            {
                if (all["data"][id] == null)
                {
                    tanks.Add(new Tank(id, "Unknown", Tier.I, "None", TankType.HEAVY));
                    continue;
                }
                tanks.Add(new Tank(id, (string)all["data"][id]["name"], IntToTier((int)all["data"][id]["tier"]), (string)all["data"][id]["images"]["preview"], StringToType((string)all["data"][id]["type"])));
            }

            return tanks;
        }

        private static Tank FindTankById(string id)
        {
            Tank tank;
            FileStream fs = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "AllTanks.json"), FileMode.OpenOrCreate, FileAccess.Write);
            using (StreamReader reader = new StreamReader(fs))
            {
                string text = reader.ReadToEnd();
                JObject all = JObject.Parse(text);
                
                    tank = all["data"][id] == null 
                        ? new Tank(id, "Unknown", Tier.I, "None", TankType.HEAVY) 
                        : new Tank(id, (string)all["data"][id]["name"], IntToTier((int)all["data"][id]["tier"]), (string)all["data"][id]["images"]["preview"],StringToType((string)all["data"][id]["type"]));
            }
            return tank;
        }
    }


}
