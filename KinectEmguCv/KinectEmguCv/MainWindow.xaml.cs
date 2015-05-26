using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Microsoft.Kinect;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Windows.Forms;

namespace KinectEmguCv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        KinectSensor sensor;
        WriteableBitmap depthBitmap;
        WriteableBitmap colorBitmap;
        DepthImagePixel[] depthPixels;
        byte[] colorPixels;

        /*The function to convert Windows Bitmap into OpenCv Image type*/
        public static Image<TColor,TDepth>ToOpenCVImage<TColor, TDepth>(this Bitmap bitmap)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return new Image<TColor, TDepth>(bitmap);
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += Window_Loaded;
            this.Closing += Window_Closing;
           // this.MouseDown += Window_MouseDown;
        }

       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(var potentialSensor in KinectSensor.KinectSensors)
            {
                if(potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (this.sensor != null)
            {
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                this.colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth,sensor.ColorStream.FrameHeight,96,96,PixelFormats.Bgr32,null);
                this.depthBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this.colorImg.Source = colorBitmap;
            }
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            Bitmap colorBmp = new Bitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight,System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            
            using(ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }

                colorPixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(colorPixels);
                
                /*Converting from Kinect Frame to OpenCv Image Type*/
                Image<Bgr, Byte> kinectColorFrame;


                ////How many pixels needed for per row basis
                //int stride = colorFrame.Width * 4;

                //colorImg.Source = BitmapSource.Create(colorFrame.Width,colorFrame.Height,96,96,PixelFormats.Bgr32,null,pixels,stride);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(sensor);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);
            sensor.Start();
        }
    }
}
