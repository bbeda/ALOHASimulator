using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ALOHASimulator.Model;

namespace ALOHASimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();  
            var config=new Configuration();
            var configWindow = new ConfigWindow(config);
            configWindow.ShowDialog();
            Channel channel = SetupChannel(config);
            this.DataContext = channel;
        }

         

        private static Channel SetupChannel(Configuration configuration)
        {          
            TimeFrame.FrameLength = new TimeSpan(0, 0, 0, 0, configuration.FrameLength);
            Computer.PacketTimeout = new TimeSpan(0, 0, 0, 0, configuration.PacketTimeout);
            Computer.PacketGenerationMinSpan = configuration.MinGenerationTime;
            Computer.PacketGenerationMaxSpan = configuration.MaxGenerationTime;
            Channel channel = null;
            if (configuration.PureAloha)
                channel = new PureAlohaChannel();
            else channel = new SlottedAlohaChannel();

           

            for (int i = 0; i < configuration.ActiveStations.Count; i++)
            {
                channel.AddComputer(new Computer());
                if (configuration.ActiveStations[i].IsActive)
                {
                    channel.SourceComputers[i].SchedulePackets(configuration.PacketsPerComputer, channel.DestinationComputers[i]);
                    channel.SourceComputers[i].IsActive = configuration.ActiveStations[i].IsActive;
                }
            }
            return channel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow();
            window.Show();
            this.Close();
        }        
    }
}
