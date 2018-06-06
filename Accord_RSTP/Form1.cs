using Accord.Video.DirectShow;
using System;
using System.Windows.Forms;
using Accord.Video;
using System.Drawing;

namespace Accord_RSTP
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private IVideoSource videoSource;
        private AsyncVideoSource asyncVideoSource;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoSource = new VideoCaptureDevice();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource.IsRunning)
                videoSource.Stop();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            VideoCaptureDeviceForm captureDeviceForm = new VideoCaptureDeviceForm();
            if(captureDeviceForm.ShowDialog(this) == DialogResult.OK)
            {
                videoSource = captureDeviceForm.VideoDevice;
                asyncVideoSource = new AsyncVideoSource(videoSource);

                asyncVideoSource.NewFrame += AsyncVideoSource_NewFrame;
                asyncVideoSource.Start();
            }
        }

        /// <summary>
        /// Either closing the videoSrouce or asyncVideoSource will make IsRunning false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStop_Click(object sender, EventArgs e)
        {       
            if (videoSource.IsRunning)
            {
                videoSource.Stop();
                pictureBox1.Image = null;
                pictureBox1.Invalidate();
            }
        }

        private void AsyncVideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap image = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = image;
        }
    }
}
