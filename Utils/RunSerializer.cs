using System;
using System.Collections.Generic;
using System.Text;
using ReSplit.Models;
using System.IO;
using System.Xml.Serialization;

namespace ReSplit.Utils
{

    public static class RunSerializer
    {
        public static RunModel Load(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(RunModel));
                using var fs = File.OpenRead(path);
                return (RunModel)serializer.Deserialize(fs)!;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load run file." + ex.Message);
                return null;
            }
        }
        public static void Save(RunModel run, string path)
        {
            var serializer = new XmlSerializer(typeof(RunModel));
            using var fs = File.Create(path);
            serializer.Serialize(fs, run);
        }
    }

}
