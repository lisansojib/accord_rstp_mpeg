using Accord.Video.DirectShow;
using System;
using System.Windows.Forms;
using Accord.Video;
using System.Drawing;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Accord_RSTP
{
    public partial class Form1 : Form
    {
        private IVideoSource videoSource;
        private AsyncVideoSource asyncVideoSource;
        private string fileName;
        private string username = "admin";
        private string password = "123456";
        private string access_token = string.Empty;

        private string accessToken = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoSource = new VideoCaptureDevice();
            accessToken = GetToken(username, password);
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

            var file = ImageToByte(eventArgs.Frame);
            Upload(file, accessToken, fileName);
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private static void Upload(byte[] file, string accessToken, string fileName)
        {
            try
            {
                var client = new WebClient();
                string boundary = "------------------------" + DateTime.Now.Ticks.ToString("x");
                client.Headers[HttpRequestHeader.ContentType] = "multipart/form-data; boundary=" + boundary;
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                var fileData = client.Encoding.GetString(file);
                var package = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}\r\n--{0}--\r\n", boundary, fileName, "image/jpeg", fileData);

                var nfile = client.Encoding.GetBytes(package);

                client.UploadData(Constants.STREAM_UPLOAD_URL, "POST", nfile);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string GetToken(string userName, string password)
        {
            var result = string.Empty;
            var accesToken = string.Empty;

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>( "grant_type", "password" ),
                new KeyValuePair<string, string>( "username", userName ),
                new KeyValuePair<string, string> ( "password", password)
            };

            var content = new FormUrlEncodedContent(pairs);

            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.PostAsync(Constants.TOKEN_URL, content).Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = response.Content.ReadAsStringAsync().Result;
                        var jsonResult = JObject.Parse(result);
                        return jsonResult.GetValue(Constants.ACCESS_TOKEN_PROPERTY).ToString();
                    }
                }
            }
            catch (Exception)
            {
                // Handle Exception later 
            }

            return string.Empty;
        }
    }
}
