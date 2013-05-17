using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PageEngineLib;
using VideoToolkitLib;
using System.Configuration;
using System.Windows;
using AxVideoToolkitLib;

namespace Virtual.Engine.Camino
{
    public class Output
    {
        public AxVxScene Scene = null;
        public IVxPageEngine PageEngine = null;
        public IVxCamera Camera = null;
        private SceneBrowserNET.SceneBrowserNETControl _sceneBrowser = null;
        private VtxXmioLib.VxMixerClass _videoMixer;
        private VtxXmioLib.VxVideoInput _videoInput;

        private bool _initialized = false;

        public IVxMesh3D FieldFrame = null;

        public string CalibrationFile = null;
        public string TelemetryFile = null;

        public string WorkingDirectory = null;

        //public IVxMesh3D BallFrame = null;

        public Output(ref AxVxScene scene, ref SceneBrowserNET.SceneBrowserNETControl sceneBrowser)
        {
            Scene = scene;
            _sceneBrowser = sceneBrowser;
        }

        public bool Initialized
        {
            get { return _initialized; }
            set { _initialized = value; }
        }

        public void InitializeScene(VX_DISPLAY_TYPES displayType, VX_FORMAT_TYPES formatType)
        {
             //init, with aa turned on since we have 3d elements
            Scene.EnableFullScreenAA = true;

            Scene.Initialize(displayType, formatType);
            //Scene.CanvasScaleToFit = 1;
            Scene.EnableInputVideo = true;

            // get the scene cast in its different forms
            IVxScene sceneInterface = (IVxScene)Scene.GetOcx();
            object sceneObject = (object)sceneInterface;
            
            if (displayType == VX_DISPLAY_TYPES.VX_DISPLAY_XMIO)
            {
                VtxXmioLib.VX_FORMAT_TYPES xmioFormatType = VtxXmioLib.VX_FORMAT_TYPES.VX_FORMAT_720P;

                switch (formatType)
                {
                    case VX_FORMAT_TYPES.VX_FORMAT_1080P_5000:
                        xmioFormatType = VtxXmioLib.VX_FORMAT_TYPES.VX_FORMAT_1080P_5000;
                        break;
                }

                _videoMixer = new VtxXmioLib.VxMixerClass();
                _videoInput = new VtxXmioLib.VxVideoInput();
                _videoMixer.Initialize2(1, 0, 1, 0, xmioFormatType, true);
                _videoMixer.ConnectVideoInput(_videoInput);
                _videoMixer.EnableOnboardCompositor = true;
                _videoInput.EnableVideo = true;

                sceneInterface.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK, 1, 0);
            }

            sceneInterface.SetCommandQueueID(0);

            // instance and init pageengine
            PageEngine = new VxPageEngine();
            PageEngine.Initialize(sceneInterface);
            PageEngine.SetWorkingDirectory(WorkingDirectory);

            // load virtual camera template
            PageEngine.LoadTemplate("VirtualCamera");
            PageEngine.ShowPage("VirtualCamera", true);
            PageEngine.DeactivateTimeline("VirtualCamera", "HOME", false);

            //PageEngine.LoadTemplate("Field");
            //PageEngine.ShowPage("Field", true);
            //PageEngine.FlushQueuedCommands();

            // setup scenebrowser
            _sceneBrowser.SetScene(ref sceneObject);
            _sceneBrowser.Refresh();

            // bind members to template
            //_templateFrame = Scene.Frames.Item("Field");
            //_rotateFrame = _templateFrame.Frames.Item("RotateFrame");
            //BallFrame = (IVxMesh3D)_templateFrame.Frames.Item("SoccerBall");
            //FieldFrame = (IVxMesh3D)_rotateFrame.Frames.Item("FieldScan");

            // turn off hit testing on example trees
            //foreach (IVxFrame it in FieldFrame.Frames)
            //{
            //    it.EnableHitTest = false;
            //}

            Camera = (IVxCamera)Scene.Frames.DescendantItem("Virtual Camera");

            Camera.NearPlane = 10;//0.00001f; // 10
            Camera.FarPlane = 100000;//1000;//100000;
            Camera.Highlight = 1;
            Camera.LookThrough = true;

            IVxPropertySet propertySet = null;
            Camera.GetPropertySet(ref propertySet, true);

            propertySet.Set("AimMethod", (int)VX_AIM_METHOD.VX_AIM_METHOD_CAM_FRAME);

            Scene.InternalComposite = VX_INTERNAL_COMPOSITE_TYPES.VX_ICOMP_LIVE_INPUT | VX_INTERNAL_COMPOSITE_TYPES.VX_ICOMP_SHAPED;

            // push updated hit test parameters to the template
            Scene.Update();

            // ready!
            _initialized = true;
        }

        //public void PlotMarker(VX_FLOAT3_TYPE intersection)
        //{
        //    if (_initialized)
        //    {
        //        VX_FLOAT3_TYPE ballPosition = intersection;
        //        BallFrame.Translate(ballPosition.x, ballPosition.y, ballPosition.z);
        //        Scene.Update();
        //    }
        //}
    }
}
