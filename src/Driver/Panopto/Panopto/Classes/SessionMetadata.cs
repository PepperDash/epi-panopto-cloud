using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto
{
    public class SessionMetadata
    {
        public int AverageRating { get; set; }
        public List<Contributor> Contributors { get; set; }
        public string Duration { get; set; }
        public string Identifier { get; set; }
        public string PublicID { get; set; }
        public int RatingCount { get; set; }
        public string SessionAbstract { get; set; }
        public object SessionGroupAbstract { get; set; }
        public string SessionGroupLongName { get; set; }
        public string SessionGroupPublicID { get; set; }
        public object SessionGroupShortName { get; set; }
        public string SessionName { get; set; }
        public string SessionPublicID { get; set; }
        public string SessionStartTime { get; set; }
        public List<Object> Timestamps { get; set; }
    }
}