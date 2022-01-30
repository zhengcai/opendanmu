using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;
using System.Net; 
using System.Net.Sockets; 

namespace DanMu
{

    public class DanMuEntry
    {
        public static bool showDanmu = true;

        public TextBlock danmu;
        public Canvas can;

        public DanMuEntry(TextBlock t, Canvas c)
        {
            this.danmu = t;
            this.can = c;
        }

        public void PushDanmu(string s)
        {
            if (showDanmu)
            {
                this.danmu.Dispatcher.Invoke(
                    new Action(
                        delegate
                        {
                            this.danmu.Text = s;
                            this.danmu.UpdateLayout();
                            DoubleAnimation doubleAnimation = new DoubleAnimation();
                            doubleAnimation.From = -this.danmu.ActualWidth;
                            doubleAnimation.To = this.can.ActualWidth + 1;
                            doubleAnimation.Duration = new Duration(TimeSpan.Parse("0:0:10"));
                            this.danmu.BeginAnimation(Canvas.RightProperty, doubleAnimation);
                        }
                    )
                );
            }
        }

        public void ClearDanmu()
        {
            this.danmu.Text = "";
            this.danmu.UpdateLayout();
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public LinkedList<DanMuEntry> danmus;
        public Socket serverSock;

        private readonly BackgroundWorker worker = new BackgroundWorker();

        private void receiving()
        {
            byte[] buffer = new byte[2048];
            int bytesRec = 0;
            while (true)
            {
                try
                {
                    bytesRec = serverSock.Receive(buffer);
                }
                catch (ObjectDisposedException)
                {
                    serverSock.Close();
                    return;
                }
                String msg = Encoding.Unicode.GetString(buffer, 0, bytesRec);
                DanMuEntry dd = danmus.First();
                danmus.RemoveFirst();
                dd.PushDanmu(msg);
                danmus.AddLast(dd);
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            IPAddress ipAddr = System.Net.IPAddress.Parse("192.168.1.64");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 9527);
            SocketPermission permission = new SocketPermission(NetworkAccess.Connect,
                TransportType.Tcp, "", SocketPermission.AllPorts);

            while (true)
            {
                serverSock = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    serverSock.Connect(ipEndPoint);
                    receiving();
                }
                catch (SocketException)
                {
                    serverSock.Close();
                }
                Thread.Sleep(1000);
            }
        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            danmus = new LinkedList<DanMuEntry>();
            danmus.AddLast(new DanMuEntry(danmu00, can00));
            danmus.AddLast(new DanMuEntry(danmu10, can10));
            danmus.AddLast(new DanMuEntry(danmu20, can20));
            danmus.AddLast(new DanMuEntry(danmu30, can30));
            danmus.AddLast(new DanMuEntry(danmu40, can40));
            danmus.AddLast(new DanMuEntry(danmu50, can50));

            danmus.AddLast(new DanMuEntry(danmu01, can01));
            danmus.AddLast(new DanMuEntry(danmu11, can11));
            danmus.AddLast(new DanMuEntry(danmu21, can21));
            danmus.AddLast(new DanMuEntry(danmu31, can31));
            danmus.AddLast(new DanMuEntry(danmu41, can41));
            danmus.AddLast(new DanMuEntry(danmu51, can51));

            danmus.AddLast(new DanMuEntry(danmu02, can02));
            danmus.AddLast(new DanMuEntry(danmu12, can12));
            danmus.AddLast(new DanMuEntry(danmu22, can22));
            danmus.AddLast(new DanMuEntry(danmu32, can32));
            danmus.AddLast(new DanMuEntry(danmu42, can42));
            danmus.AddLast(new DanMuEntry(danmu52, can52));

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //var textBox = sender as TextBox;
            //textBox.Text = "在此输入弹幕,回车发送";
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text = "";
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            var textBox = sender as TextBox;
            byte[] byteData = Encoding.Unicode.GetBytes(textBox.Text);
            if (this.serverSock.Connected)
            {
                this.serverSock.Send(byteData);
            }
            textBox.Text = "";
            textBox.UpdateLayout();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DanMuEntry.showDanmu = !DanMuEntry.showDanmu;
            if (!DanMuEntry.showDanmu)
            {
                foreach (DanMuEntry danmu in this.danmus)
                {
                    danmu.ClearDanmu();
                }
            }
        }
    }
}
