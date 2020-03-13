﻿using ChatObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public frmMain(string username, string fullname)
        {
            InitializeComponent();
            panelAside.VerticalScroll.Value = panelAside.VerticalScroll.Maximum;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            //chatbox1.Visible = false;
            this.username = username;
            this.fullname = fullname;
            this.lblNameUser.Text = fullname;
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
                }
            }
            catch (Exception)
            {
                throw;
            }
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
                        String data = DateTime.Now + "-" + chatData.Header.SessionFrom + ":" + chatData.Payload.Data;

                        //invokeTextBox(this.)
                    }
                }
                catch (Exception e)
                {

                }
                System.Threading.Thread.Sleep(200);
            }
        }

        public void invokeTextBox()
        {
            //if (t.InvokeRequired)
            //{
            //    //t.Invoke(new Action<TextBox, string>(invokeTextBox), new Object[] { t, s });
            //}
            //else
            //{
            //    //t.AppendText(s);
            //}
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
    }
}