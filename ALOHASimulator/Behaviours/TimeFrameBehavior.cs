using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows.Controls;
using ALOHASimulator.Controls;
using ALOHASimulator.Model;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media;

namespace ALOHASimulator.Behaviors
{
    public class TimeFrameBehavior : Behavior<ChannelPanel>
    {
        ChannelPanel associatedObject;

        public TimeFrameBehavior()
        {
            
        }

        public bool DisplayTimeFrameLines
        {
            get;
            set;
        }

        private  Dictionary<string, TimeFrameControl> controlsStoryboards = new Dictionary<string, TimeFrameControl>();
        private  Dictionary<string, Line> lines = new Dictionary<string, Line>();


        public Channel Channel
        {
            get { return (Channel)GetValue(ChannelProperty); }
            set { SetValue(ChannelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Channel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChannelProperty =
            DependencyProperty.Register("Channel", typeof(Channel), typeof(TimeFrameBehavior), new UIPropertyMetadata(ChannelChanged));
        
        protected static void ChannelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var channel = e.NewValue as Channel;
            if (channel == null)
                return;
            
            var behavior = (sender as TimeFrameBehavior);
            
            channel.TimeFrames.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(behavior.TimeFrames_CollectionChanged);
        }

        void TimeFrames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (TimeFrame frame in e.NewItems)
                    {
                        AddFrameControl(frame);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (TimeFrame frame in e.OldItems)
                    {
                        for (int i = 0; i < associatedObject.Children.Count; i++)
                        {
                            if ((associatedObject.Children[i] as FrameworkElement).DataContext == frame)
                            {
                                associatedObject.Children.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }

        private void AddFrameControl(TimeFrame frame)
        {
            if (!frame.IsControlFrame)
            {
                var rowHeight = associatedObject.ActualHeight / frame.Channel.SourceComputers.Count;
                var computerIndex = frame.Channel.SourceComputers.IndexOf(frame.Packet.Source);
                var frameControl = new TimeFrameControl() { DataContext = frame };
                Canvas.SetLeft(frameControl, 0);
                Canvas.SetTop(frameControl, rowHeight * computerIndex + (rowHeight - frameControl.ActualHeight) / 2);
                var timeRatio = TimeFrame.FrameLength.TotalMilliseconds / GetAnimationDuration().TotalMilliseconds;
                frameControl.Width = associatedObject.ActualWidth * timeRatio;
                associatedObject.Children.Add(frameControl);
                var storyboard = GetStoryboard(frameControl);
                controlsStoryboards.Add(storyboard.Name, frameControl);
                storyboard.Begin();
            }

            if (frame.IsControlFrame || !(frame.Channel is SlottedAlohaChannel))
            {
                var line = CreateLine();
                line.DataContext = frame;
                associatedObject.Children.Add(line);
                var lineStoryBoard = GetStoryboardForLine(line);
                lines.Add(lineStoryBoard.Name, line);
                lineStoryBoard.Begin();
            }
        }


        public Line CreateLine()
        {
            Line line = new Line();
            line.Y2 = associatedObject.ActualHeight;
            line.StrokeThickness = 1;
            line.Stroke = Brushes.Black;
            line.StrokeDashArray = new DoubleCollection(new double[] { 1, 2 });


            return line;
        }

        public  Storyboard GetStoryboardForLine(Line line)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Name = "storyboardLine_" + Guid.NewGuid().ToString().Substring(0, 5);

            DoubleAnimation animation = GetAnimation();
            Storyboard.SetTargetProperty(animation, new PropertyPath("X1"));
            DoubleAnimation animation1 = GetAnimation();
            Storyboard.SetTargetProperty(animation1, new PropertyPath("X2"));
            Storyboard.SetTarget(storyboard, line);

            storyboard.Children.Add(animation);
            storyboard.Children.Add(animation1);

            storyboard.Completed += new EventHandler(lineStoryboard_Completed);

            return storyboard;

        }

         void lineStoryboard_Completed(object sender, EventArgs e)
        {
            var clock = sender as ClockGroup;
            if (clock == null)
                return;
            var storyboard = clock.Timeline as Storyboard;


            var line = lines[storyboard.Name];
            lines.Remove(storyboard.Name);

            associatedObject.Children.Remove(line);

            var timeFrame = line.DataContext as TimeFrame;
            if (timeFrame == null)
                return;
            if (timeFrame.IsControlFrame)
                timeFrame.Channel.DeliverFrame(timeFrame);
        }
        public  Storyboard GetStoryboard(TimeFrameControl frameControl)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Name = "storyboard_" + Guid.NewGuid().ToString().Substring(0, 5);

            DoubleAnimation animation = GetAnimation();
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(storyboard, frameControl);


            storyboard.Children.Add(animation);
            storyboard.Completed += new EventHandler(storyboard_Completed);

            return storyboard;

        }

        private  DoubleAnimation GetAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation(associatedObject.ActualWidth, GetAnimationDuration());
            return animation;
        }

        private  TimeSpan GetAnimationDuration()
        {
            return Computer.PacketTimeout - new TimeSpan(0, 0, 0, 0, 100);
        }

         void storyboard_Completed(object sender, EventArgs e)
        {
            var clock = sender as ClockGroup;
            if (clock == null)
                return;
            var storyboard = clock.Timeline as Storyboard;
            var frameControl = controlsStoryboards[storyboard.Name];
            controlsStoryboards.Remove(storyboard.Name);



            var timeFrame = frameControl.DataContext as TimeFrame;
            if (timeFrame == null)
                return;

            timeFrame.Channel.DeliverFrame(timeFrame);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.associatedObject = AssociatedObject;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
