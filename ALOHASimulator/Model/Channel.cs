using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using GalaSoft.MvvmLight;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Data;

namespace ALOHASimulator.Model
{
    public class Channel : ObservableObject
    {
        protected ObservableCollection<Computer> _sourceComputers;
        public ObservableCollection<Computer> SourceComputers
        {
            get
            {
                if (_sourceComputers == null)
                {
                    this._sourceComputers = new ObservableCollection<Computer>();
                }
                return _sourceComputers;
            }
        }

        protected ObservableCollection<Computer> _destinationComputers;
        public ObservableCollection<Computer> DestinationComputers
        {
            get
            {
                if (_destinationComputers == null)
                {
                    this._destinationComputers = new ObservableCollection<Computer>();
                }
                return _destinationComputers;
            }
        }

        private ObservableCollection<TimeFrame> _timeFrames;
        public ObservableCollection<TimeFrame> TimeFrames
        {
            get
            {
                if (_timeFrames == null)
                {
                    _timeFrames = new ObservableCollection<TimeFrame>();

                }
                return _timeFrames;
            }
        }

        private static Channel _noChannel;
        public static Channel NoChannel
        {
            get
            {
                if (_noChannel == null)
                {
                    _noChannel = new Channel();
                }
                return _noChannel;
            }
        }

        public int ComputerCount
        {
            get
            {
                return this.SourceComputers.Count;
            }
        }

        public virtual void AddComputer(Computer computer)
        {
            this.SourceComputers.Add(computer);
            computer.Channel = this;
            var destination = new Computer() { Channel = this };
            this.DestinationComputers.Add(destination);
            computer.PropertyChanged += computer_PropertyChanged;
        }

        public virtual void Stop()
        {
            while (this.TimeFrames.Count > 0)
                this.TimeFrames.RemoveAt(0);            
        }

        public string Name { get; set; }

        protected Channel()
        {
            this.TimeFrames.CollectionChanged += TimeFrames_CollectionChanged;
        }


        public void DeliverFrame(TimeFrame frame)
        {
            this._timeFrames.Remove(frame);

            if (!frame.IsControlFrame && !frame.IsLost)
            {
                frame.Packet.Destination.ReceivePacket(frame.Packet);
            }
        }

        void TimeFrames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var collection = sender as ObservableCollection<TimeFrame>;
            if (collection == null)
                return;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (TimeFrame frame in e.NewItems)
                    {
                        foreach (var oldFrame in collection)
                        {
                            if (frame == oldFrame)
                                continue;
                            if (frame.IsCollision(oldFrame))
                            {
                                frame.IsLost = true;
                                oldFrame.IsLost = true;
                                break;
                            }
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }

        void computer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var computer = sender as Computer;
            if (computer == null)
                return;
            Expression<Func<bool>> expression = () => computer.IsPacketAviable;
            var propertyName = ((MemberExpression)expression.Body).Member.Name;

            if (e.PropertyName == propertyName)
                ComputerPacketAviable(computer);

            Expression<Func<bool>> isActiveProperty = () => computer.IsPacketAviable;
            propertyName = ((MemberExpression)expression.Body).Member.Name;
            if (e.PropertyName == propertyName)
                this.DestinationComputers[this.SourceComputers.IndexOf(computer)].IsActive = computer.IsActive;
        }

        protected virtual void ComputerPacketAviable(Computer computer)
        {
        }

        protected void AddTimeFrame(TimeFrame frame)
        {
            frame.Channel = this;
            this.TimeFrames.Add(frame);
        }


    }
}
