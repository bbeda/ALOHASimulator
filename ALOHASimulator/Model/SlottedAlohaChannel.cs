using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Diagnostics;

namespace ALOHASimulator.Model
{
    public class SlottedAlohaChannel : Channel
    {
        private static int count;
        DispatcherTimer timer = new DispatcherTimer();        

        public SlottedAlohaChannel()
        {
            count++;
            Debug.WriteLine("Created" + count.ToString());
            this.Name = count.ToString();
            timer.Interval = TimeFrame.FrameLength;
            timer.Tick += new EventHandler(timer_Tick);
            timer.IsEnabled = true;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.IsEnabled = false;
            TimeFrame frame = new TimeFrame(Packet.NoPacket);
            frame.IsControlFrame = true;
            this.AddTimeFrame(frame);

            foreach (var computer in this.SourceComputers)
            {
                if (computer.IsPacketAviable)
                {
                    var packet = Packet.NoPacket;
                    if (computer.GetPacket(out packet))
                    {
                        var packetFrame = new TimeFrame(packet);                        
                        this.AddTimeFrame(packetFrame);
                    }
                }
            }

            timer.IsEnabled = true;
        }

        public override void Stop()
        {
            base.Stop();
            this.timer.IsEnabled = false;
        }
    }
}
