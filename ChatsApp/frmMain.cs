﻿using ChatObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatsApp
{
    public partial class frmMain : Form
    {
        private ChatClientControl chatClient = null;
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private const int cGrip = 22;      
        private const int cCaption = 55;

        private bool fullScreen = false;

        private string username = "";
        private string fullname = "";

        private string hostAddress = "";
        private int iPort = 0;

        private int dock = 0;

        public delegate void AddButton(string fullname);
        public AddButton myDelegate;

        public delegate void RefreshPanelAside();
        public RefreshPanelAside reFreshPanel;

        public frmMain _main;

        public frmMain(string username, string fullname)
        {
            InitializeComponent();
            _main = this; 
            panelAside.VerticalScroll.Value = panelAside.VerticalScroll.Maximum;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            //chatbox1.Visible = false;
            this.username = username;
            this.fullname = fullname;
            this.lblNameUser.Text = fullname;
            myDelegate = new AddButton(AddButtonMethod);
            reFreshPanel = new RefreshPanelAside(RefreshPanelAsideMethod);
        }

        public void getConfig()
        {
            this.hostAddress = ConfigurationManager.AppSettings["hostAddress"];
            this.iPort = Int32.Parse(ConfigurationManager.AppSettings["iPort"]);
        }

        public void connect(string hostAddress, int iPort)
        {
            try
            {
                this.chatClient = new ChatClientControl();
                if (this.chatClient.connect(hostAddress, iPort))
                {
                    ChatJoinObject chatJoin = new ChatJoinObject();

                    chatJoin.Username = this.username;
                    this.chatClient.sendDataObject(chatJoin);

                    ThreadStart threadStart = new ThreadStart(runRecvData);
                    Thread thread = new Thread(threadStart);

                    thread.IsBackground = true;
                    thread.Start();

                    threadStart = new ThreadStart(sendRefresh);
                    thread = new Thread(threadStart);

                    thread.IsBackground = true;
                    thread.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void sendRefresh()
        {
            while (this.chatClient != null)
            {
                try
                {
                    ChatHeaderObject header = new ChatHeaderObject()
                    {
                        Header = Header.Refresh,
                        SessionFrom = this.username
                    };
                    ChatPayloadObject payload = new ChatPayloadObject()
                    {

                    };
                    ChatDataObject chatData = new ChatDataObject()
                    {
                        Header = header,
                        Payload = payload
                    };
                    this.chatClient.sendDataObject(chatData);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                System.Threading.Thread.Sleep(5000);
            }
        }

        private Dictionary<string, string> getAllUser(string allUser)
        {
            Dictionary<string, string> dictAllUser = new Dictionary<string, string>();
            dictAllUser = allUser.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(part => part.Split('='))
               .ToDictionary(split => split[0], split => split[1]);
            return dictAllUser;
        }

        private void runRecvData()
        {
            while(this.chatClient != null)
            {
                try
                {
                    Object o = chatClient.recvDataObject();
                    if (o != null)
                    {
                        ChatDataObject chatData = (ChatDataObject)o;
                        if (chatData.Header.Header == Header.Refresh)
                        {
                            string allUser = chatData.Payload.Data;
                            Dictionary<string, string> dictUsers = getAllUser(allUser);
                            invokeButton(_main, dictUsers);
                        }
                        //invokeTextBox(this.)
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                System.Threading.Thread.Sleep(200);
            }
        }

        private void invokeButton(frmMain main,Dictionary<string, string> dictUsers)
        {
            if (dictUsers != null)
            {
                main.Invoke(main.reFreshPanel);
                foreach (KeyValuePair<string, string> user in dictUsers)
                {
                    main.Invoke(main.myDelegate, new Object[] { user.Key });
                }
            }
        }

        private void RefreshPanelAsideMethod()
        {
            List<Button> buttons = panelAside.Controls.OfType<Button>().ToList();
            foreach (Button btn in buttons)
            {
                btn.Click -= new EventHandler(this.button2_Click); //It's unnecessary
                panelAside.Controls.Remove(btn);
                btn.Dispose();
            }
            dock = 0;
        }

        private void AddButtonMethod(string fullname)
        {
            Button button = new Button();
            button.Text = fullname;
            button.BackColor = button2.BackColor;
            button.UseVisualStyleBackColor = false;
            button.Height = button2.Height;
            button.Width = button2.Width;
            button.FlatStyle = button2.FlatStyle;
            button.FlatAppearance.BorderColor = button2.FlatAppearance.BorderColor;
            button.FlatAppearance.BorderSize = button2.FlatAppearance.BorderSize;
            button.FlatAppearance.MouseOverBackColor = button2.FlatAppearance.MouseOverBackColor;
            button.FlatAppearance.MouseDownBackColor = button2.FlatAppearance.MouseDownBackColor;
            button.Image = button2.Image;
            button.TextAlign = button2.TextAlign;
            button.ImageAlign = button2.ImageAlign;
            button.Location = button2.Location;
            button.Anchor = button2.Anchor;
            button.Dock = button2.Dock;
            button.Top = dock;
            panelAside.Controls.Add(button);
            dock += button.Height;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rc = new Rectangle(this.ClientSize.Width - cGrip, this.ClientSize.Height - cGrip, cGrip, cGrip);
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, rc);
            rc = new Rectangle(0, 0, this.ClientSize.Width, cCaption);
            e.Graphics.FillRectangle(Brushes.DarkBlue, rc);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x2)
                        m.Result = (IntPtr)0x1;
                    Point pos = new Point(m.LParam.ToInt32());
                    pos = this.PointToClient(pos);
                    if (pos.Y < cCaption)
                    {
                        m.Result = (IntPtr)2;  // HTCAPTION
                        return;
                    }
                    if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                    {
                        m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                        return;
                    }
                    return;
            }

            base.WndProc(ref m);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void btnScreen_Click(object sender, EventArgs e)
        {
            if (fullScreen == false)
            {
                this.WindowState = FormWindowState.Maximized;
                fullScreen = true;
                this.btnScreen.BackgroundImage = global::ChatsApp.Properties.Resources.icons8_normal_screen_50px;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                fullScreen = false;
                this.btnScreen.BackgroundImage = global::ChatsApp.Properties.Resources.icons8_fit_to_width_26px;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private string genRoomName(string toUser)
        {
            string roomName = "";
            roomName = this.username + toUser;
            return roomName;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            getConfig();
            connect(hostAddress, iPort);
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
