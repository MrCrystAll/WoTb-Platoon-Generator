using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Windows.UI.Popups;
using Newtonsoft.Json.Linq;

namespace WotGenC
{
    public class Requester
    {
        public static Stream RequestTanks(string id, string token)
        {
            string request = $"https://api.wotblitz.eu/wotb/tanks/stats/?application_id=f34ba3232ab0297c9d23660a7e18544c&account_id={id}&access_token={token}&fields=-max_xp%2C+-account_id%2C+-mark_of_mastery%2C+-in_garage_updated%2C+-all%2C+-frags%2C+-last_battle_time%2C+-max_frags%2C+-battle_life_time&in_garage=1";
            HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(request);
            httprequest.Method = WebRequestMethods.Http.Get;
            HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse();

            return response.GetResponseStream();
        }

        public static void RequestAllTanks(string endFile)
        {
            string request = "https://api.wotblitz.eu/wotb/encyclopedia/vehicles/?application_id=f34ba3232ab0297c9d23660a7e18544c&fields=-suspensions%2C+-description%2C+-engines%2C+-prices_xp%2C+-next_tanks%2C+-modules_tree%2C+-nation%2C+-is_premium%2C+-cost%2C+-default_profile%2C+-guns%2C+-turrets%2C";
            HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(request);
            httprequest.Method = WebRequestMethods.Http.Get;
            HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse();

            Stream stream = response.GetResponseStream();

            if (response.StatusCode != HttpStatusCode.OK) return;
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;


            FileStream fs = File.Open(Path.Combine(Directory.GetCurrentDirectory(), endFile), FileMode.OpenOrCreate, FileAccess.Write);
            while ((bytesRead = stream.Read(buffer, 0, bufferSize)) != 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
            fs.Close();
        }

        public static Stream RequestTankStats(Tank t, Player player)
        {
            string request =
                $"https://api.wotblitz.eu/wotb/tanks/stats/?application_id=f34ba3232ab0297c9d23660a7e18544c&account_id={player.Id}&access_token={player.Token}&fields=-in_garage_updated&tank_id={t.Id}";
            HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(request);
            httprequest.Method = WebRequestMethods.Http.Get;
            HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse();
            
            return response.GetResponseStream();
        }

        public static UInt64 GetInt(string server, string cmdString)
        {
            NamedPipeClientStream s = new NamedPipeClientStream(server);
            s.Connect();

            if (!s.IsConnected) return 0;
            
            byte[] result = new byte[8];
            byte[] command = System.Text.Encoding.ASCII.GetBytes(cmdString);
            byte[] size = BitConverter.GetBytes((int)command.Length);

            byte[] fullcommand = new byte[1];

            fullcommand[0] = 1; // execute string
            fullcommand = fullcommand.Concat(size).ToArray();
            fullcommand = fullcommand.Concat(command).ToArray();

            fullcommand = fullcommand.Concat(BitConverter.GetBytes((long)0)).ToArray(); //the 'parameter' value

            s.Write(fullcommand, 0, fullcommand.Length);

            s.Read(result, 0, 8);

            s.Close();

            return BitConverter.ToUInt64(result, 0);

        }

    }

    public static class RequestVerifyier
    {
        public static void CheckResult(string response)
        {

            string content = response;
            var jObject = JObject.Parse(content);

            string errorMessage = (string)jObject["error"]?["message"];
            string additionnalInfo = "Unknown problem";

            if (errorMessage == "INVALID_ACCESS_TOKEN")
            {
                additionnalInfo = "Try to refresh the access token";
            }

            if ((string)jObject["status"] == "error")
            {
                throw new Exception($"Error for the request : {errorMessage} ({additionnalInfo})");
            }
        }
    }
}
