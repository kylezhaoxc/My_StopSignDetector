using BetterTogether.Bluetooth;
using BetterTogether.Device;
using BetterTogether.Media;
using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;


using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.Features2D;
using System.IO;

namespace My_StopSignDetector
{
    public partial class Form1 : Form
    {
        Queue<double> statusqueue = new Queue<double>();
        Queue<System.Drawing.Point> centerpos = new Queue<System.Drawing.Point>();
        System.Drawing.Point center;
        // object that will be used to synchronize UI & capture thread
        private Object syncRoot = new Object();
        //value of the matched area that will be used to filter mismatches and far-away-matches
        public int areathreshold = 500;
        // threshold value of positive votes, decresing the interferences.
        public int surr_acc = 50;

        public System.Drawing.Bitmap modelpic_l;
        public System.Drawing.Bitmap modelpic_s;
        // two Bitmap resources that will hold the most recent captured image
        public System.Drawing.Bitmap largeBitmap1;
        public System.Drawing.Bitmap smallBitmap1;
        // a timer that will run on the UI thread, to update the UI periodically
        private System.Windows.Forms.Timer uiUpdateTimer;
        long time = 0; double area = 0; Image<Bgr, Byte> match_res;
        
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            uiUpdateTimer = new System.Windows.Forms.Timer();
            // run every 10 ms (100 frames / second)
            uiUpdateTimer.Interval = 10;
            uiUpdateTimer.Tick += uiUpdateTimer_TickHandler;
            backgroundWorker1.RunWorkerAsync();
            uiUpdateTimer.Start();
            match.Visible = false; nomatch.Visible = false;
        }
        // this method needs to be called on form load
        private void uiUpdateTimer_TickHandler(object sender, EventArgs e)
        {
            lock (syncRoot)
            {//if there is input image from the video stream, show the frame
                if (largeBitmap1 != null) pictureBox1.Image = smallBitmap1;
                //if there is a match
                if (match_res != null) 
                {//for surrounding matching, don't show the unstable rectangle.
                    if (match_sur.Checked) pictureBox3.Image = largeBitmap1;
                    //for item matching, show the matching result.
                    else pictureBox3.Image = match_res.ToBitmap();
                }
                //deciding what additional information to show 
                label7.Text = time.ToString();
                label5.Text = area.ToString("f2");
                //matching or not are not decided by one single image, but a serie of frames.
                //keep the length of the queue 100
                    statusqueue.Enqueue(area);
                    if (statusqueue.Count > 100) statusqueue.Dequeue();
                    // num represents the positive match number in the queue.
                int num =checkqueue();
                //while matched , start considering the direction, using the same theory.
                if (num > surr_acc)
                {
                   centerpos.Enqueue(center);
                   if (centerpos.Count > 200) centerpos.Dequeue();
                   string  direct;
                   TellDirection(out direct);
                   this.label8.Text = direct;
                   this.match.Visible = true; this.nomatch.Visible = false;
               
               }
                //while no-match
               else { this.match.Visible = false; this.nomatch.Visible = true; this.label8.Text = "Direction:N/A"; }
            } 
        }
        public void TellDirection(out string direction)
        {
            int xmax = match_sur.Checked ? 360 : 420;
            int xmin = match_sur.Checked ? 280 : 220;
            direction="N/A";
            int leftvote=0,rightvote=0,centervote=0;
            foreach (System.Drawing.Point center in centerpos)
            {
                if (center.X > xmax) rightvote++;
                if (center.X < xmin) leftvote ++;
                if (center.X > xmin && center.X < xmax) centervote++;
            }
            if (leftvote > 100) direction = "Turn Left!";
            if (rightvote > 100) direction = "Turn Right!";
            if (centervote > 100) direction = "Go Straight!";
        }
        private int checkqueue()
        {
            int positivematch = 0; double sum = 0; int count=0;
            foreach (double area in statusqueue)
            {
                positivematch += area > areathreshold ? 1 : 0;
                sum += area; count++;
            }
           
            return positivematch;
        }
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Capture cam = new Capture();
            
                while (cam.Grab())
                {
                    System.Drawing.Bitmap b1 = cam.QueryFrame().ToBitmap();
                    System.Drawing.Bitmap bs = cam.QuerySmallFrame().ToBitmap();
                    lock (syncRoot)
                    {
                        largeBitmap1 = b1;
                        smallBitmap1 = bs;
                        if(modelpic_l!=null)
                        match_res = DrawMatches.Draw(new Image<Gray, Byte>(modelpic_l), new Image<Gray, Byte>(largeBitmap1), out time,out area,areathreshold,out center);
                    }
            }
        }
        private void btnscreenshot_Click(object sender, EventArgs e)
        {
            modelpic_l = largeBitmap1;
            modelpic_s = smallBitmap1;
            pictureBox2.Image =modelpic_s;

        }

        private void match_sur_CheckedChanged(object sender, EventArgs e)
        {
            surr_acc = 10;
            match_itm.Checked = false;
        }

        private void match_itm_CheckedChanged(object sender, EventArgs e)
        {
            surr_acc = 50;
            match_sur.Checked = false;
        }
    }
}
    

