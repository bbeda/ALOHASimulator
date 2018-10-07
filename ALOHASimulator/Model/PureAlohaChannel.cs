using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace ALOHASimulator.Model
{
    public class PureAlohaChannel : Channel
    {
        Dictionary<Computer, DispatcherTimer> processSchedules = new Dictionary<Computer, DispatcherTimer>();
        
        public PureAlohaChannel()
        {
            this.Name = "Aloha";
        }

        public override void Stop()
        {
            base.Stop();
            foreach (var kvp in processSchedules)
            {
                kvp.Value.IsEnabled = false;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            if (timer == null)
                return;
            timer.IsEnabled = false;
            var computer = processSchedules.Keys.Where(key => processSchedules[key] == timer).FirstOrDefault();
            var packet = Packet.NoPacket;
            if (computer.GetPacket(out packet))
            {
                var frame = new TimeFrame(packet);
                this.AddTimeFrame(frame);
            }
        }
        protected override void ComputerPacketAviable(Computer computer)
        {
            if (!processSchedules.Keys.Contains(computer))
                return;
            processSchedules[computer].IsEnabled = true;
            
        }

        public override void AddComputer(Computer computer)
        {
            base.AddComputer(computer);
            var timer = new DispatcherTimer
            {
                 Interval=new TimeSpan(0,0,0,0,10),
            };            
            timer.Tick+=new EventHandler(timer_Tick);
            processSchedules.Add(computer, timer);
        }
    }
}
