using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace ALOHASimulator.Model
{
    public class Computer : ObservableObject
    {
        public static int PacketGenerationMinSpan = 50;
        public static  int PacketGenerationMaxSpan = 2500;

        private static Computer _noComputer;
        public static Computer NoComputer
        {
            get
            {
                if (_noComputer == null)
                {
                    _noComputer = new Computer();
                }
                return _noComputer;
            }
        }

        public ICollectionView Packets { get; private set; }

        public static TimeSpan PacketTimeout = new TimeSpan(0, 0, 8);
        static Random randomNumberGenerator=new Random(DateTime.Now.Millisecond);
        ObservableCollection<Packet> _packets;
        DispatcherTimer _packetScheduleTimer;
        DispatcherTimer _packetTimeoutChecker;
        Dictionary<Packet, DateTime> _sentNoAckPackets;
        private int packetsComputed = 0;

        private bool _isPacketAviable;
        public bool IsPacketAviable
        {
            get
            {
                return this._isPacketAviable;
            }
            private set
            {
                if (_isPacketAviable != value)
                {
                    _isPacketAviable = value;
                    if (value)
                    {
                        _packetScheduleTimer.IsEnabled = false;
                        _packetScheduleTimer.Interval = new TimeSpan(0, 0, 0, 0, randomNumberGenerator.Next(PacketGenerationMinSpan, PacketGenerationMaxSpan));
                    }
                    else
                    {
                        _packetScheduleTimer.IsEnabled = true;
                    }
                    RaisePropertyChanged(() => this.IsPacketAviable);
                    this.RaisePropertyChanged(() => this.ActivePacketInfo);
                }
            }
        }


        private string _name;
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    this.RaisePropertyChanged(() => this.Name);
                }
            }
        }

        public void SchedulePackets(int packetCount, Computer destination)
        {
            for (int i = 0; i < packetCount; i++, packetsComputed++)
            {
                this._packets.Add(new Packet(this, destination) { Info = packetsComputed.ToString(), });
            }
        }

        public bool GetPacket(out Packet packet)
        {
            packet = Packet.NoPacket;

            if (this.IsPacketAviable)
            {
                packet = this._packets.ElementAt(0);
                this._packets.RemoveAt(0);
                _sentNoAckPackets.Add(packet, DateTime.Now);
                this.IsPacketAviable = false;
                return true;
            }
            return false;
        }

        public void AckPacket(Packet packet)
        {
            if (this._sentNoAckPackets.ContainsKey(packet))
            {
                this._sentNoAckPackets.Remove(packet);
            }
        }

        public Channel Channel
        {
            get
            {
                return _channel;
            }
            set
            {
                if (this._channel != value)
                {
                    this._channel = value;
                    RaisePropertyChanged(() => this.Channel);
                }
            }
        }
        private Channel _channel;
        public Computer()
        {
            _packets = new ObservableCollection<Packet>();            

            _packetScheduleTimer = new DispatcherTimer();
            _packetScheduleTimer.Tick += SchedulePacketCallback;

            _packetTimeoutChecker = new DispatcherTimer()
            {
                Interval = PacketTimeout,
            };
            _packetTimeoutChecker.Tick += CheckPacketsTimeouts;

            _sentNoAckPackets = new Dictionary<Packet, DateTime>();
            this.Name = string.Format("Computer_{0}", Guid.NewGuid().ToString().Substring(0, 5));

            Packets = new CollectionView(this._packets);
        }



        void SchedulePacketCallback(object sender, EventArgs e)
        {
            if (_packets.Count != 0)
                this.IsPacketAviable = true;
            else
            {
                _packetScheduleTimer.IsEnabled = false;                
                _packetScheduleTimer.Interval = new TimeSpan(0, 0, 0, 0, randomNumberGenerator.Next(PacketGenerationMinSpan, PacketGenerationMaxSpan));
                _packetScheduleTimer.IsEnabled = true;
            }
        }

        public string ActivePacketInfo
        {
            get
            {
                if (this.IsPacketAviable && this._packets.Count > 0)
                    return this._packets.ElementAt(0).Info;
                else return string.Empty;
            }
        }
        void CheckPacketsTimeouts(object sender, EventArgs e)
        {
            _packetTimeoutChecker.IsEnabled = false;
            for (int i = 0; i < _sentNoAckPackets.Keys.Count; i++)
            {
                var packet = _sentNoAckPackets.Keys.ElementAt(i);
                var elapsed = DateTime.Now - _sentNoAckPackets[packet];
                if (elapsed > PacketTimeout)
                {
                    this._sentNoAckPackets.Remove(packet);
                    this._packets.Add(packet);
                    i--;
                }
            }
            _packetTimeoutChecker.IsEnabled = true;
        }

        private bool _isActive;
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }
            set
            {
                if (this._isActive != value)
                {
                    this._isActive = value;
                    ChangeActiveState(value);
                    this.RaisePropertyChanged(() => this.IsActive);
                }
            }
        }

        private void ChangeActiveState(bool isActive)
        {
            if (IsActive)
                _packetScheduleTimer.Interval = new TimeSpan(0, 0, 0, 0, randomNumberGenerator.Next(PacketGenerationMinSpan, PacketGenerationMaxSpan));
            this._packetScheduleTimer.IsEnabled = isActive;
            this._packetTimeoutChecker.IsEnabled = isActive;           

        }

        public void ReceivePacket(Packet packet)
        {
            Debug.WriteLine(packet.Info);
            packet.Source.AckPacket(packet);
            this.AcceptedPackets.Add(packet);
        }

        private ObservableCollection<Packet> _acceptedPackets;
        public ObservableCollection<Packet> AcceptedPackets
        {
            get
            {
                if (_acceptedPackets == null)
                    _acceptedPackets = new ObservableCollection<Packet>();
                return _acceptedPackets;
            }
        }
    }
}
