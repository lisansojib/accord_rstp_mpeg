using Accord.Video.DirectShow;
using System;
using System.Windows.Forms;
using Accord.Video;
using System.Drawing;
using System.Net;

namespace Accord_RSTP
{
    public partial class Form1 : Form
    {
        private IVideoSource videoSource;
        private AsyncVideoSource asyncVideoSource;
        private string fileName;

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
            fileName = "Channel" + "_" + DateTime.Now.Ticks.ToString() + ".jpeg";

            VideoCaptureDeviceForm captureDeviceForm = new VideoCaptureDeviceForm();
            if(captureDeviceForm.ShowDialog(this) == DialogResult.OK)
            {
                videoSource = captureDeviceForm.VideoDevice;
                var source = videoSource.Source;
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

            try
            {
                var file = ImageToByte(eventArgs.Frame);
                var client = new WebClient();
                string boundary = "------------------------" + DateTime.Now.Ticks.ToString("x");
                client.Headers[HttpRequestHeader.ContentType] = "multipart/form-data; boundary=" + boundary;
                var fileData = client.Encoding.GetString(file);
                var package = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}\r\n--{0}--\r\n", boundary, fileName, "image/jpeg", fileData);

                var nfile = client.Encoding.GetBytes(package);

                byte[] resp = client.UploadData(Constants.StreamUploadUrl, "POST", nfile);
            }
            catch(WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
