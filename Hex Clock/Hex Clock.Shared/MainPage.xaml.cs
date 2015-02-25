using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Hex_Clock
{
    public struct HexColour
    {
        // Regex: This pattern matches hex codes in these two formats:
        // #000000 (no alpha value) and #FF000000 (alpha value at front).
        const string HEX_PATTERN = @"^\#([a-fA-F0-9]{6}|[a-fA-F0-9]{8})$";

        const int LENGTH_WITH_ALPHA = 8;

        Color _color;

        public HexColour(string hexCode)
        {
            if (hexCode == null)
            {
                throw new ArgumentNullException("hexCode");
            }

            if (!Regex.IsMatch(hexCode, HEX_PATTERN))
            {
                throw new ArgumentException("Format must be #000000 or #FF000000 (no extra whitespace)", "hexCode");
            }

            // shave off '#' symbol
            hexCode = hexCode.TrimStart('#');

            // if no alpha value specified, assume no transparency (0xFF)
            if (hexCode.Length != LENGTH_WITH_ALPHA)
                hexCode = String.Format("FF{0}", hexCode);

            _color = new Color();
            _color.A = byte.Parse(hexCode.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            if (_color.A < 50)
                _color.A = 50;
            _color.R = byte.Parse(hexCode.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            _color.G = byte.Parse(hexCode.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            _color.B = byte.Parse(hexCode.Substring(6, 2), NumberStyles.AllowHexSpecifier);
        }

        public byte A
        {
            get { return _color.A; }
            set { _color.A = value; }
        }

        public byte R
        {
            get { return _color.R; }
            set { _color.R = value; }
        }

        public byte G
        {
            get { return _color.G; }
            set { _color.G = value; }
        }

        public byte B
        {
            get { return _color.B; }
            set { _color.B = value; }
        }

        // Implicit cast from HexColor to Color
        public static implicit operator Color(HexColour hexColor)
        {
            return hexColor._color;
        }

        // Implicit cast from Color to HexColor
        public static implicit operator HexColour(Color color)
        {
            HexColour c = new HexColour();
            c._color = color;
            return c;
        }

        // Just like with Color, ToString() prints out the hex value of the
        // color in #ARGB format (example: #FF000000) by default.
        public override string ToString()
        {
            return ToString(true);
        }

        // I don't always need the alpha value, so I added an overload here
        // that lets me return the hex value in #RBG format (example: #000000).
        public string ToString(bool includeAlpha)
        {
            if (includeAlpha)
            {
                return _color.ToString();
            }
            else
            {
                return String.Format("#{0}{1}{2}", _color.R.ToString("x2"), _color.G.ToString("x2"), _color.B.ToString("x2"));
            }
        }
    }
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer _timer;
        public MainPage()
        {
            this.InitializeComponent();
            #if WINDOWS_PHONE_APP
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            #endif
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUI(this, null);

            Storyboard sb = (Storyboard)this.Resources["IntialAnimation"];
            sb.BeginTime = TimeSpan.FromSeconds(0.1);
            sb.Begin();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.5);
            _timer.Tick += RefreshUI;
            _timer.Start();
            try
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Tick += timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 0, 1);
                timer.Start();
                timer_Tick(null, null);             //Call an initial tick
            }
            catch { }
        }
        public void RefreshUI(object sender, object e)
        {
            DateTime dt = DateTime.Now;
            DateText.Text = dt.ToString();
            int seconds = dt.Second;
            int minutes = dt.Minute;
            int hour = dt.Hour;

            int year = dt.Year;

            // Time 
            if (TimeHours.Text != dt.Hour.ToString())
            {
                TimeHours.Text = dt.Hour.ToString();
            }

            if (TimeMinutes.Text != dt.Minute.ToString("D2"))
            {
                TimeMinutes.Text = dt.Minute.ToString("D2");
            }

            if (TimeSeconds.Text != dt.Second.ToString("D2"))
            {
                TimeSeconds.Text = dt.Second.ToString("D2");
            }
        }
        void timer_Tick(object sender, object e)
        {
            try
            {
                //Get time for this tick
                int hour = System.DateTime.Now.Hour;
                int minute = System.DateTime.Now.Minute;
                int second = System.DateTime.Now.Second;
                //Ensure that the hex value has 6 digits by padding with zeroes.
                //TODO: do this more efficiently
                StringBuilder sb = new StringBuilder("#");
                if (hour < 10)
                {
                    sb.Append("0" + hour.ToString());
                }
                else
                {
                    sb.Append(hour.ToString());
                }
                if (minute < 10)
                {
                    sb.Append("0" + minute.ToString());
                }
                else
                {
                    sb.Append(minute.ToString());
                }
                if (second < 10)
                {
                    sb.Append("0" + second.ToString());
                }
                else
                {
                    sb.Append(second.ToString());
                }
                string hexTime = sb.ToString();
                sb.Remove(0, 1);                            //remove # character for displayed time
                for (int i = sb.Length - 2; i > 0; i -= 2)     //22:41:10 formatting
                    sb.Insert(i, ":");
                string currentTime = sb.ToString();
                //Update lblClock
                //clocktime.Text = currentTime;
                //Update background color according to time value
                HexColour color = new HexColour(hexTime);
                SolidColorBrush bgBrush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                LayoutRoot.Background = bgBrush;
            }
            catch { }
        }
        private void test()
        {
        }
    }
}
