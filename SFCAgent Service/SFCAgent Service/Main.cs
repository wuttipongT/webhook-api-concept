using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Quobject.SocketIoClientDotNet.Client;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Microsoft.Reporting.WinForms;
using System.Printing;
using System.Drawing.Printing;

namespace SFCAgent_Service
{
    public partial class Main : Form
    {

        //Socket socket;
        String ip;
        String name;
        String server;
        Boolean closeFlg = false;
        string version;

        private enum MessageType
        {
            TYPE_DEFAULT,
            TYPE_INFO,
            TYPE_DANGER,
            TYPE_WARNING,
            TYPE_SUCCESS,
            TYPE_PRIMARY
        }

        Dictionary<String, MessageType> type = new Dictionary<String, MessageType>();
        public Main()
        {
            /*SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

    */

            InitializeComponent();

            type.Add("TYPE_DEFAULT", MessageType.TYPE_DEFAULT);
            type.Add("TYPE_INFO", MessageType.TYPE_INFO);
            type.Add("TYPE_DANGER", MessageType.TYPE_DANGER);
            type.Add("TYPE_WARNING", MessageType.TYPE_WARNING);
            type.Add("TYPE_SUCCESS", MessageType.TYPE_SUCCESS);
            type.Add("TYPE_PRIMARY", MessageType.TYPE_PRIMARY);

            setType(MessageType.TYPE_DANGER);

            //label1.Text = "SN: Tester\nWas NG for 3 times.\nBy IT Team.";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            version = fvi.FileVersion;

            this.notifyIcon.Text = "SFC Message Center V " + version;
            label1.Text = "SFC Message Center\nVersion " + version;

            label1.Top = (this.Height - label1.Height) / 2 - 30;

            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();
            notifyIcon.Visible = true;

            this.Menu = new MainMenu();
            MenuItem item = new MenuItem("Help");
            this.Menu.MenuItems.Add(item);
            item.MenuItems.Add("About SFC Message Center", new EventHandler(about_Click));
            //item.MenuItems.Add("Open", new EventHandler(Open_Click));

            ContextMenu menu = new ContextMenu();
            MenuItem menuItem = new MenuItem("Exit", new EventHandler(mnuExit_Click));
            menu.MenuItems.Add(menuItem);

            notifyIcon.ContextMenu = menu;
            this.ip = Dns.GetHostAddresses(Dns.GetHostName()).First(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();

            this.name = System.Environment.MachineName;

            this.server = System.Configuration.ConfigurationManager.AppSettings["Server"];

            socketIo();

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!this.closeFlg)
            {
                e.Cancel = true;

                this.WindowState = FormWindowState.Minimized;
                Hide();
                notifyIcon.Visible = true;
                this.closeFlg = false;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            Hide();
            notifyIcon.Visible = true;
        }

        private void about_Click(object sender, EventArgs e)
        {
            label1.Text = "SFC Message Center\nVersion " + version;
            label1.Top = (this.Height - label1.Height) / 2 - 30;
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            this.closeFlg = true;
            this.Close();
        }

        private void socketIo()
        {

            var socket = IO.Socket(this.server);

            //Event
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                List<String> printer = getPrinter();
                string defaultPrinter = getDefaultPrinter();

                var data = new
                {
                    ip = this.ip,
                    name = this.name,
                    printers = printer,
                    defaultPrinter = defaultPrinter
                };

                var jsonFormat = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                socket.Emit("recieveUserName", jsonFormat);
                notifyIcon.BalloonTipText = "Connected Server " + server;
                notifyIcon.ShowBalloonTip(1000);

            });

            var that = this;
            socket.On("message", (data) =>
            {
                Invoke(new Action(() =>
                {
                    that.Show();
                    that.WindowState = FormWindowState.Normal;
                    notifyIcon.Visible = false;

                    var o = (JObject)data;

                    //tboMessage.Text = (string)o.GetValue("message");
                    String message = (string)o.GetValue("message");
                    //message = message.Replace(System.Environment.NewLine, "\n");
                    label1.Text = message;
                    String strType = (string)o.GetValue("type");
                    if (strType != null)
                    {
                        setType(type[strType]);
                    }
                    //SFCForm.MainWindow fm = new SFCForm.MainWindow();
                    label1.Top = (this.Height - label1.Height) / 2 - 30;

                }));
            });

            socket.On("print", (data) =>
            {
                try
                {
                    Invoke(new Action(() =>
                    {

                        notifyIcon.BalloonTipText = "Client Printed.";
                        notifyIcon.ShowBalloonTip(1000);

                        var o = (JObject)data;
                        //JArray jarray = (JArray)o.GetValue("JOBS");
                        //JToken entry = (JToken)jarray[0];
                        //var jobs = (String)entry[0]["DocumentName"];
                        LocalReport localReport = new LocalReport();

                        foreach (JObject job in (JArray)o.GetValue("JOBS")) {
                            
                            localReport.ReportPath = (String)job.GetValue("FilePath");
                            foreach (JObject doc in (JArray)job.GetValue("DocumentDatas")) {
                               
                                try
                                {
                                    ReportParameter Param = new ReportParameter();
                                    Param.Name = (String)doc.GetValue("Parameter");
                                    Param.Values.Add((String)doc.GetValue("Value"));
                                    localReport.SetParameters(new ReportParameter[] { Param });
                                }
                                catch (LocalProcessingException e)
                                {
                                    MessageBox.Show(e.InnerException.Message);
                                    break;
                                }
         
                            }

                            if ((String)job.GetValue("PrintServerID") != (String)job.GetValue("PrinterName"))
                            {
                                localReport.SetPrinterName((String)job.GetValue("PrinterName"));
                                localReport.PrintToPrinter();
                            }
                            else {
                                localReport.PrintToPrinter(); //default printer output
                            }
                        }

                       // LocalReport localReport = new LocalReport();
                       // localReport.ReportPath = Application.StartupPath + "\\FGRECEIVEDBILL_REPORT.rdl";
/*
                        foreach (var x in (JObject)o.GetValue("item"))
                        {
                            ReportParameter Param = new ReportParameter();
                            Param.Name = (String)x.Key;
                            Param.Values.Add((String)x.Value);
                            localReport.SetParameters(new ReportParameter[] { Param });
                        }

                        localReport.PrintToPrinter((String)o.GetValue("printer"));
*/

                    }));
                }
                catch (Exception e)
                {

                    MessageBox.Show(e.Message);
                }

            });

            socket.On(Socket.EVENT_ERROR, (error) => {
                // ...

                Invoke(new Action(() =>
                {
                    notifyIcon.BalloonTipText = "Connect Error " + server;
                    notifyIcon.ShowBalloonTip(1000);
                }));
            });

        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;

        }

        private void setType(MessageType type = MessageType.TYPE_DEFAULT)
        {
            if (type == MessageType.TYPE_DEFAULT)
            {
                Color color = System.Drawing.ColorTranslator.FromHtml("#BDBDBD");
                BackColor = color;
            }
            else if (type == MessageType.TYPE_PRIMARY)
            {
                Color color = System.Drawing.ColorTranslator.FromHtml("#3c8dbc");
                BackColor = color;
            }
            else if (type == MessageType.TYPE_INFO)
            {
                Color color = System.Drawing.ColorTranslator.FromHtml("#d9edf7");
                BackColor = color;
            }
            else if (type == MessageType.TYPE_SUCCESS)
            {
                Color color = System.Drawing.ColorTranslator.FromHtml("#36B97D");
                BackColor = color;
            }
            else if (type == MessageType.TYPE_WARNING)
            {
                Color color = System.Drawing.ColorTranslator.FromHtml("#F87D10");
                BackColor = color;
            }
            else if (type == MessageType.TYPE_DANGER)
            {
                Color color = System.Drawing.ColorTranslator.FromHtml("#FF4E38");
                BackColor = color;
            }

        }

        private List<string> getPrinter()
        {
            List<string> printersList = new List<string>();
            var server = new PrintServer();
            var queues = server.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local });

            foreach (var queue in queues)
            {
                printersList.Add(queue.FullName);
            }

            return printersList;
        }

        private string getDefaultPrinter()
        {
            PrinterSettings printerSettings = new PrinterSettings();
            return printerSettings.PrinterName;
        }
    }
}
