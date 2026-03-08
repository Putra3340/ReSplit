using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
namespace ReSplit.Models
{
    [XmlRoot("Run")]
    public class RunModel
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        public string GameName { get; set; }
        public string CategoryName { get; set; }
        public string Platform { get; set; }

        public Metadata Metadata { get; set; }

        [XmlArray("Segments")]
        [XmlArrayItem("Segment")]
        public List<Segment> Segments { get; set; } = new();
    }

    public class Metadata
    {
        public int AttemptCount { get; set; }
    }

    public class Segment
    {
        public string? Id { get; set; }
        public string Name { get; set; }

        [XmlArray("SplitTimes")]
        [XmlArrayItem("SplitTime")]
        public List<SplitTime> SplitTimes { get; set; } = new();
    }

    public class SplitTime
    {
        [XmlAttribute("name")]
        public string Comparison { get; set; }

        [XmlElement(IsNullable = true)]
        public string RealTime { get; set; }   // parse manually later
    }


}
