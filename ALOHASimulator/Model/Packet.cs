using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace ALOHASimulator.Model
{
    public class Packet : ObservableObject
    {
        public string Info
        {
            get;
            set;
        }

        public Computer Source { get; set; }
        public Computer Destination { get; set; }

        private static Packet _noPacket;
        public static Packet NoPacket
        {
            get
            {
                if (_noPacket == null)
                {
                    _noPacket = new Packet(null,null);
                }

                return _noPacket;
            }
        }

        public Packet(Computer source, Computer destination)
        {
            this.Source = source;
            this.Destination = destination;
            this.Info = Guid.NewGuid().ToString().Substring(0, 5);
        }
    }
}
