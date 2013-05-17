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
using System.Configuration;
using VideoToolkitLib;
using PageEngineLib;
using Virtual.Engine.Sockets;
using Virtual.Engine.Camino;

namespace Virtual.Engine.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        private CameraDataWindow _cameraDataWindow = null;
        private OutputWindow _outputWindow = null;
        private Output _previewScene = null;
        private Output _outputScene = null;

        private CommandProcessor _commandProcessor = null;

        private delegate SocketCommand ProcessCommandCallback(SocketCommand CommandToProcess);

        #endregion

        #region Public Members

        public Listener[] MyListener;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            string numberOfListeners = ConfigurationManager.AppSettings["NumberOfListeners"];

            if (String.IsNullOrEmpty(numberOfListeners))
            {
                numberOfListeners = "1";
            }

            MyListener = new Listener[Convert.ToInt32(numberOfListeners)];

            for (int i = 0; i < Convert.ToInt32(numberOfListeners); i++)
            {
                // (bsk) for now, port starts at 1000, and increments by 2
                string port = Convert.ToString(1000 + (i * 2));
                MyListener[i] = initializeListener(ConfigurationManager.AppSettings["ListenerPort"].ToString(), i.ToString());
            }

            _previewScene = new Output(ref vxScene, ref sceneBrowser);
            //_previewScene.InitializeScene(VX_DISPLAY_TYPES.VX_DISPLAY_VGA, VX_FORMAT_TYPES.VX_FORMAT_1080P_5000);
            //_previewScene.Scene.CanvasScaleToFit = 1;

            _outputWindow = new OutputWindow();
            _outputScene = new Output(ref _outputWindow.vxScene, ref _outputWindow.sceneBrowser);
            _outputWindow.Show();

            //_cameraDataWindow = new CameraDataWindow(ref _outputScene.Scene, ref _outputScene.Camera, ref _previewScene.Scene, ref _previewScene.Camera);

            _commandProcessor = new CommandProcessor(ref _outputScene, ref _previewScene);

            // bind events to scene
            vxScene.MouseMoveEvent += new AxVideoToolkitLib._IVxSceneEvents_MouseMoveEventHandler(vxScene_MouseMoveEvent);
            vxScene.MouseDownEvent += new AxVideoToolkitLib._IVxSceneEvents_MouseDownEventHandler(vxScene_MouseDownEvent);
        }

        #endregion
        
        #region Private Methods

        private void vxScene_MouseMoveEvent(object sender, AxVideoToolkitLib._IVxSceneEvents_MouseMoveEvent e)
        {
            if (!_previewScene.Initialized)
                return;

            // use bounding box primitive hit test since it is quick

            IVxFrame hitFrame = null;
            int handleIntersectionIndex = 0;

            //_previewScene.Scene.HitTest(e.x, e.y, VX_HITTEST_OPT_TYPES.VX_HITTEST_OPT_BOUNDINGBOX, ref hitFrame, ref handleIntersectionIndex);
            _previewScene.Scene.HitTest(e.x, e.y, VX_HITTEST_OPT_TYPES.VX_HITTEST_OPT_PRIMATIVES, ref hitFrame, ref handleIntersectionIndex);

            if (hitFrame != null)
            {
                // we hit our field!
                // show the cross hairs
                Mouse.OverrideCursor = Cursors.Cross;
            }
            else
            {
                // no intersection, normal cursor
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void vxScene_MouseDownEvent(object sender, AxVideoToolkitLib._IVxSceneEvents_MouseDownEvent e)
        {
            if (!_previewScene.Initialized)
                return;

            IVxFrame hitFrame = null;
            int handleIntersectionIndex = 0;

            VX_FLOAT3_TYPE intersection = new VX_FLOAT3_TYPE();
            VX_FLOAT3_TYPE intersectionNormal = new VX_FLOAT3_TYPE();

            // hit with the primitives of the mesh so that we have a precise hit
            // [use hittest2, since we want the intersection and normal back]
            vxScene.HitTest2(e.x, e.y, VX_HITTEST_OPT_TYPES.VX_HITTEST_OPT_PRIMATIVES, ref hitFrame, ref handleIntersectionIndex, ref intersection, ref intersectionNormal);

            //if (hitFrame == (IVxFrame)_previewScene.FieldFrame)
            //{
                // we got a hit on the field, update the position!
                VX_FLOAT3_TYPE ballPosition = intersection;

                //_previewScene.PlotMarker(intersection);

                //_previewScene.BallFrame.Translate(ballPosition.x, ballPosition.y, ballPosition.z);
                //vxScene.Update();

                //send to the real scene
                //_outputScene.PlotMarker(intersection);
                //_outputWindow.PlotMarker(intersection);

                //send the coordinates to the interface app
                foreach(Listener listener in MyListener)
                {
                    SocketCommand commandToSend = new SocketCommand();
                    commandToSend.CommandID = Guid.NewGuid().ToString();
                    commandToSend.Command = CommandType.HitLocation;
                    commandToSend.Timestamp = DateTime.Now;

                    commandToSend.Parameters = new List<CommandParameter>();
                    commandToSend.Parameters.Add(new CommandParameter("Location", intersection.x + "," + intersection.y + "," + intersection.z));

                    listener.SendData(commandToSend);
                }
            //}
        }

        private void takeToAir()
        {

        }

        //scene is being initialized when command is received from front end

        //private void mnuInitVGA_Click(object sender, RoutedEventArgs e)
        //{
        //    _outputScene.InitializeScene(VX_DISPLAY_TYPES.VX_DISPLAY_VGA, VX_FORMAT_TYPES.VX_FORMAT_720P);
        //}

        //private void mnuInit720AJA_Click(object sender, RoutedEventArgs e)
        //{
        //    _outputScene.InitializeScene(VX_DISPLAY_TYPES.VX_DISPLAY_AJA, VX_FORMAT_TYPES.VX_FORMAT_720P);
        //}

        //private void mnuInit720XMIO_Click(object sender, RoutedEventArgs e)
        //{
        //    _outputScene.InitializeScene(VX_DISPLAY_TYPES.VX_DISPLAY_XMIO, VX_FORMAT_TYPES.VX_FORMAT_720P);
        //}

        //private void mnuInit1080_50XMIO_Click(object sender, RoutedEventArgs e)
        //{
        //    _outputScene.InitializeScene(VX_DISPLAY_TYPES.VX_DISPLAY_XMIO, VX_FORMAT_TYPES.VX_FORMAT_1080I_5000);
        //}

        private void mnuCameraData_Click(object sender, RoutedEventArgs e)
        {
            if (_cameraDataWindow == null)
            {
                _cameraDataWindow = new CameraDataWindow(ref _outputScene, ref _previewScene);
            }

            _cameraDataWindow.Show();
        }

        private void mnuTiming_Click(object sender, RoutedEventArgs e)
        {
            IVxScene scene = _outputScene.Scene.GetIVxScene();

            AdjustVideoView formAdjustVideo = new AdjustVideoView();

            formAdjustVideo.Scene = scene;
            if (formAdjustVideo.ShowDialogInt() == System.Windows.Forms.DialogResult.OK)
            {
                formAdjustVideo.ShowDialog();
            }

        }

        #endregion

        #region Listener

        private Listener initializeListener(string Port, string ID = "")
        {
            Listener initListener = new Listener(Port, ID);

            initListener.DataArrival += new Listener.DataArrivalEventHandler(dataArrival);
            initListener.Connected += new Listener.ConnectionHandler(connectedFrom);

            return initListener;
        }

        private void dataArrival(SocketCommand CommandToProcess, string ID)
        {
            SocketCommand returnCommand = null;
            // send the command back to the main thread for processing...

            try
            {
                ProcessCommandCallback d = new ProcessCommandCallback(_commandProcessor.ProcessCommand);
                returnCommand = (SocketCommand)this.Dispatcher.Invoke(d, new object[] { CommandToProcess });
            }
            catch (Exception ex)
            {
                returnCommand = new SocketCommand();
                returnCommand.Command = Virtual.Engine.Sockets.CommandType.CommandFailed;
                returnCommand.Parameters = new List<CommandParameter>();
                returnCommand.Parameters.Add(new CommandParameter("command", CommandToProcess.ToString()));
            }

            SendData(returnCommand, ID);

        }

        private static void connectedFrom()
        {
            //AddStatus("Listener", "Talker has connected");
        }

        public void SendData(SocketCommand commandObjecttoSend, string ID)
        {
            int sendOn = 0;

            if (!string.IsNullOrEmpty(ID))
            {
                sendOn = Convert.ToInt32(ID);
            }

            try
            {
                MyListener[sendOn].SendData(commandObjecttoSend);
            }
            catch (Exception ex)
            {
                //Debug.Print(ex.InnerException.ToString()); 
            }
        }

        public void SendData(SocketCommand commandObjecttoSend)
        {
            try
            {
                for (int i = 0; i < MyListener.Length; i++)
                {
                    MyListener[i].SendData(commandObjecttoSend);
                }
            }
            catch (Exception ex)
            {
                //Debug.Print(ex.InnerException.ToString()); 
            }
        }

        #endregion
    }
}
