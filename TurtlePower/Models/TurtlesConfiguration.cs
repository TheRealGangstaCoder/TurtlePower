using System;
using System.Collections.Generic;

namespace Turtle
{
    public class TurtlesConfiguration
    {
        public List<Turtle> Turtle { get; set; }
    }

    public class Turtle
    {
        public Uri DestinationUri { get; set; }

        public Uri NextHopUri { get; set; }
    }
}
