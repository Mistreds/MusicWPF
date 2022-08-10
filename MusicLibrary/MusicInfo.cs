using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace MusicLibrary
{
    public class MusicInfo
    {
        public string Track { get; set; }
        public string Singer { get; set; }
        public byte[] Image { get; set; }
        public MusicInfo(string track, string singer)
        {
            Track=track;
            Singer=singer;
        }
        public MusicInfo() 
        {
            Track="";
            Singer="";
        }
        public override string ToString()
        {
            return $"{Track} {Singer}";
        }
    }
}
