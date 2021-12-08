using System.Collections.Generic;

namespace Turtle
{
    public class TurtlesConfiguration
    {
        public List<Turtle> Turtle { get; set; }
    }

    public class Turtle
    {
        public string Uri { get; set; }
        public NextHop NextHop { get; set; }
    }

    public enum NextHop
    { 
        Turtle = 0,
        Destination =1
    }

}
