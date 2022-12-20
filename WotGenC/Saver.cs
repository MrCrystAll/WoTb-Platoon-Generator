using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml;
using Windows.Storage.Streams;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;

namespace WotGenC
{
    public class Saver
    {
        public static void SaveBackup(string path, ListOfTanks content)
        {
            using FileStream stream =  new FileStream(path, FileMode.Create);
            DataContractSerializer serializer = new DataContractSerializer(typeof(ListOfTanks));
            serializer.WriteObject(stream, content);

            // using StreamWriter streamWriter = new StreamWriter(path);
            //
            // //Meta part
            // streamWriter.Write("{");
            // streamWriter.Write("\"meta\": {" +
            //                    $"\"count\": {content.Count}}},");
            //
            // streamWriter.Write("\"data\": {");
            // uint i = 0;
            // foreach (var tank in content)
            // {
            //     streamWriter.Write($"\"{i}\":{{");
            //     streamWriter.Write($"\"Id\": {tank.Id}," +
            //                        $"\"Name\" : \"{tank.Nom}\"," +
            //                        $"\"Tier\" : {tank.Te:D}," +
            //                        $"\"Image\": \"{tank.Image}\"");
            //
            //     i++;
            //     streamWriter.Write(i == content.Count ? "}" : "},");
            // }
            // streamWriter.Write("}}");
        }
    }
}