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
using System.Windows.Shapes;
using Microsoft.Win32;
using VideoToolkitLib;
using PageEngineLib;
using System.Windows.Threading;
using AxVideoToolkitLib;
using System.Timers;

using Virtual.Engine.Camino;

namespace Virtual.Engine.Views
{
    /// <summary>
    /// Interaction logic for CameraDataWindow.xaml
    /// </summary>
    public partial class CameraDataWindow : Window
    {
        #region Private Members

        //private IVxScene _scene = null;
        //private IVxCamera _camera = null;

        private Output _scene = null;
        //private IVxCamera _camera = null;

        private Output _livePreviewScene = null;
        //private IVxCamera _livePreviewCamera = null;

        private VirtualCameraLibrary.Units _units = VirtualCameraLibrary.Units.Meters;

        private VirtualCameraLibrary.VirtualCameraTracker _cameraTracker = null;

        private Timer _cameraDataTimer = null;

        #endregion

        #region Constructor

        public CameraDataWindow(ref Output scene, ref Output prevScene)
        {
            InitializeComponent();

            _scene = scene;
            //_camera = scene.Camera;

            _livePreviewScene = prevScene;
            //_livePreviewCamera = prevScene.Camera;

            _cameraDataTimer = new Timer();
            _cameraDataTimer.Interval = 1000;
            _cameraDataTimer.Elapsed += new ElapsedEventHandler(cameraDataTimer_Elapsed);
            _cameraDataTimer.Enabled = true;
        }

        #endregion

        #region Private Methods

        private void btnNetworkCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_scene.Camera == null)
            {
                string message = "No virtual camera was found in the scene";
                System.Console.WriteLine(message);
                System.Diagnostics.Debug.WriteLine(message);
            }

            if (_cameraTracker != null)
            {
                _cameraTracker.StopTracking();
                _cameraTracker.RemoveAllVirtualScenes();
            }

            VirtualCameraLibrary.NetworkVirtualCameraTracker.NetworkVirtualCameraTracker cameraTracker =
                new VirtualCameraLibrary.NetworkVirtualCameraTracker.NetworkVirtualCameraTracker();

            cameraTracker.AddVirtualScene(_scene.Scene.GetIVxScene(), _scene.Camera, _units, false);
            cameraTracker.AddVirtualScene(_livePreviewScene.Scene.GetIVxScene(), _livePreviewScene.Camera, _units, false);
            cameraTracker.StartTracking(this.multicastIpTextBox.Text, short.Parse(this.portTextbox.Text), true);

            _cameraTracker = cameraTracker;

            this.btnNetworkCapture.IsEnabled = false;
            this.btnSDICapture.IsEnabled = false;
            this.btnSerialCapture.IsEnabled = false;
            this.btnStopCapture.IsEnabled = true;
        }

        private void btnSDICapture_Click(object sender, RoutedEventArgs e)
        {
            //if (_camera == null)
            //{
            //    string message = "No virtual camera was found in the scene";
            //    System.Console.WriteLine(message);
            //    System.Diagnostics.Debug.WriteLine(message);
            //}

            if (_cameraTracker != null)
            {
                _cameraTracker.StopTracking();
                _cameraTracker.RemoveAllVirtualScenes();
            }

            VirtualCameraLibrary.SDIVirtualCameraTracker.SDIVirtualCameraTracker cameraTracker = new VirtualCameraLibrary.SDIVirtualCameraTracker.SDIVirtualCameraTracker();

            cameraTracker.AddVirtualScene(_scene.Scene.GetIVxScene(), _scene.Camera, _units, false);
            cameraTracker.AddVirtualScene(_livePreviewScene.Scene.GetIVxScene(), _livePreviewScene.Camera, _units, false);

            cameraTracker.StartTracking(_scene.Scene.GetIVxScene(), _scene.CalibrationFile, _scene.TelemetryFile);

            _cameraTracker = cameraTracker;

            this.btnNetworkCapture.IsEnabled = false;
            this.btnSDICapture.IsEnabled = false;
            this.btnSerialCapture.IsEnabled = false;
            this.btnStopCapture.IsEnabled = true;
        }

        //private void btnSDICapture_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_camera == null)
        //    {
        //        string message = "No virtual camera was found in the scene";
        //        System.Console.WriteLine(message);
        //        System.Diagnostics.Debug.WriteLine(message);
        //    }

        //    if (_cameraTracker != null)
        //    {
        //        _cameraTracker.StopTracking();
        //        _cameraTracker.RemoveAllVirtualScenes();
        //    }

        //    OpenFileDialog fileDialog = new OpenFileDialog();
        //    fileDialog.Title = "Select Calibration File";

        //    bool? result = fileDialog.ShowDialog();            

        //    if (result == true)
        //    {
        //        string calibrationFile = fileDialog.FileName;

        //        fileDialog.Title = "Select Telemetry File";

        //        result = fileDialog.ShowDialog();

        //        if (result == true)
        //        {
        //            string telemetryFile = fileDialog.FileName;

        //            VirtualCameraLibrary.SDIVirtualCameraTracker.SDIVirtualCameraTracker cameraTracker =
        //                new VirtualCameraLibrary.SDIVirtualCameraTracker.SDIVirtualCameraTracker();

        //            cameraTracker.AddVirtualScene(_scene, _camera, _units, false);
        //            cameraTracker.AddVirtualScene(_livePreviewScene, _livePreviewCamera, _units, false);

        //            cameraTracker.StartTracking(_scene, calibrationFile, telemetryFile);

        //            _cameraTracker = cameraTracker;

        //            this.btnNetworkCapture.IsEnabled = false;
        //            this.btnSDICapture.IsEnabled = false;
        //            this.btnSerialCapture.IsEnabled = false;
        //            this.btnStopCapture.IsEnabled = true;
        //        }
        //    }
        //}

        private void btnSerialCapture_Click(object sender, RoutedEventArgs e)
        {
            //if (_scene.Camera == null)
            //{
            //    string message = "No virtual camera was found in the scene";
            //    System.Console.WriteLine(message);
            //    System.Diagnostics.Debug.WriteLine(message);
            //}

            if (_cameraTracker != null)
            {
                _cameraTracker.StopTracking();
                _cameraTracker.RemoveAllVirtualScenes();
            }

            //OpenFileDialog fileDialog = new OpenFileDialog();
            //bool? result = fileDialog.ShowDialog();

            //if (result == true)
            //{
            //    string calibrationFile = fileDialog.FileName;

            //    result = fileDialog.ShowDialog();

            //    if (result == true)
            //    {
            //        string telemetryFile = fileDialog.FileName;

                    VirtualCameraLibrary.SerialVirtualCameraTracker.SerialVirtualCameraTracker cameraTracker =
                        new VirtualCameraLibrary.SerialVirtualCameraTracker.SerialVirtualCameraTracker();

                    cameraTracker.AddVirtualScene(_scene.Scene.GetIVxScene(), _scene.Camera, _units, false);
                    cameraTracker.AddVirtualScene(_livePreviewScene.Scene.GetIVxScene(), _livePreviewScene.Camera, _units, false);

                    cameraTracker.StartTracking(_scene.Scene.GetIVxScene(), 4, _scene.CalibrationFile, _scene.TelemetryFile);

                    _cameraTracker = cameraTracker;

                    this.btnNetworkCapture.IsEnabled = false;
                    this.btnSDICapture.IsEnabled = false;
                    this.btnSerialCapture.IsEnabled = false;
                    this.btnStopCapture.IsEnabled = true;
            //    }
            //}
        }

        private void btnStopCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_scene.Camera == null)
            {
                string message = "No virtual camera was found in the scene";
                System.Console.WriteLine(message);
                System.Diagnostics.Debug.WriteLine(message);
            }

            if (_cameraTracker != null)
            {
                _cameraTracker.StopTracking();
                _cameraTracker.RemoveAllVirtualScenes();
            }

            this.btnNetworkCapture.IsEnabled = true;
            this.btnSDICapture.IsEnabled = true;
            this.btnSerialCapture.IsEnabled = true;
            this.btnStopCapture.IsEnabled = false;
        }

        private void cameraDataTimer_Elapsed(object sender, EventArgs e)
        {
            if (_cameraTracker != null && _cameraTracker.IsTracking)
            {
                VirtualCameraLibrary.VirtualCameraTracker.CameraStats stats = _cameraTracker.Stats;

                this.Dispatcher.Invoke((Action)delegate ()
                {
                    updateCameraStats(stats);
                });
            }
            //else
            //{
                //this.packetRateValueLabel.Content = "N/A";
            //}
        }

        private void updateCameraStats(VirtualCameraLibrary.VirtualCameraTracker.CameraStats stats)
        {
            // check to see the dropped frames
            int val = 0;
            _scene.Scene.GetDebugValue("OutputDrops", ref val);
            this.droppedOutputFramesValueLabel.Content = val.ToString();

            this.packetRateValueLabel.Content = stats.SceneStats[0].ModelRate.ToString();
            this.missedPacketsValueLabel.Content = stats.SceneStats[0].SkippedModels.ToString();
            //this.droppedPacketsValueLabel.Content = stats.SceneStats[0].DroppedModels.ToString();
            this.queuedModelsValueLabel.Content = stats.SceneStats[0].QueuedModels.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        #endregion

    }
}
