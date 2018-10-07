using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace ALOHASimulator.Model
{
    public class Configuration : ObservableObject
    {
        public Configuration()
        {
            this.FrameLength = 250;
            this.MaxGenerationTime = 2500;
            this.MinGenerationTime = 100;
            this.PacketTimeout = 8000;
            this.PacketsPerComputer = 20;
            for (int i = 0; i < 5; i++)
            {
                ActiveStations.Add(new StationState() { Name = "Statie " + (i + 1).ToString(), IsActive = true });
            }
        }

        private int _frameLength;
        public int FrameLength
        {
            get
            {
                return _frameLength;
            }
            set
            {
                if (_frameLength != value)
                {
                    _frameLength = value;
                    this.RaisePropertyChanged(() => this.FrameLength);
                }
            }
        }
        private int _packetTimeout;
        public int PacketTimeout
        {
            get
            {
                return _packetTimeout;
            }
            set
            {
                if (_packetTimeout != value)
                {
                    _packetTimeout = value;
                    this.RaisePropertyChanged(() => this.PacketTimeout);
                }
            }
        }

        private int _minGenerationTime;
        public int MinGenerationTime
        {
            get
            {
                return _minGenerationTime;
            }
            set
            {
                if (_minGenerationTime != value)
                {
                    _minGenerationTime = value;
                    this.RaisePropertyChanged(() => MinGenerationTime);
                }
            }
        }

        private int _maxGenerationTime;
        public int MaxGenerationTime
        {
            get
            {
                return _maxGenerationTime;
            }
            set
            {
                if (_maxGenerationTime != value)
                {
                    _maxGenerationTime = value;
                    this.RaisePropertyChanged(() => MaxGenerationTime);
                }
            }
        }


        private List<StationState> _activeStations;
        public List<StationState> ActiveStations
        {
            get
            {
                if (_activeStations == null)
                {
                    _activeStations = new List<StationState>();
                }
                return _activeStations;
            }
        }

        private bool _pureAloha;
        public bool PureAloha
        {
            get { return _pureAloha; }
            set
            {
                if (_pureAloha != value)
                {
                    _pureAloha = value;
                    this.RaisePropertyChanged(()=>this.PureAloha);
                    this.RaisePropertyChanged(() => this.SlottedAloha);
                }
            }

        }

        public bool SlottedAloha
        {
            get
            {
                return !this.PureAloha;
            }
            set
            {
                if (this._pureAloha != !value)
                {
                    this.PureAloha = !value;
                }
            }
        }

        private int _packetsPerComputer;
        public int PacketsPerComputer
        {
            get
            {
                return _packetsPerComputer;
            }
            set
            {
                if (_packetsPerComputer != value)
                {
                    _packetsPerComputer = value;
                    this.RaisePropertyChanged(() => this.PacketsPerComputer);
                }
            }
        }

    }

    public class StationState : ObservableObject
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
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

        private bool _isActive;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    this.RaisePropertyChanged(() => this.IsActive);
                }
            }
        }
    }
}
