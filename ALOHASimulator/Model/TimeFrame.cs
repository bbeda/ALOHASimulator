using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace ALOHASimulator.Model
{
    public class TimeFrame : ObservableObject
    {
        public static TimeSpan FrameLength = new TimeSpan(0, 0, 0, 0, 250);
        public static TimeSpan LifeDuration = new TimeSpan(0, 0, 0, 5);

        public bool IsControlFrame { get; set; }

        public Channel Channel { get; set; }

        private DateTime _creationDate;
        public DateTime CreationDate
        {
            get
            {
                return _creationDate;
            }
        }

        public DateTime EdgeTime
        {
            get
            {
                return this.CreationDate + FrameLength;
            }
        }

        public TimeFrame(Packet packet)
        {
            this._creationDate = DateTime.Now;
            this.Packet = packet;
        }


        public Packet Packet { get; private set; }

        public bool IsCollision(TimeFrame frame)
        {
            if (frame.IsControlFrame || IsControlFrame)
                return false;
            if ((this.CreationDate <= frame.CreationDate && this.EdgeTime >= frame.CreationDate) || (this.CreationDate <= frame.EdgeTime && this.EdgeTime >= frame.EdgeTime))
            {
                return true;
            }

            return false;
        }


        private bool _isLost;
        public bool IsLost
        {
            get
            {
                return _isLost;
            }

            set
            {
                if (_isLost != value)
                {
                    _isLost = value;
                    RaisePropertyChanged(() => this.IsLost);
                }
            }
        }
    }
}
