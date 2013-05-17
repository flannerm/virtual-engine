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
using System.Windows.Forms;
using System.Windows.Threading;
using VideoToolkitLib;

namespace Virtual.Engine.Views
{
    /// <summary>
    /// Interaction logic for AdjustVideoView.xaml
    /// </summary>
    public partial class AdjustVideoView : Window
    {
        public IVxScene Scene;

        private DispatcherTimer _timer;

        public AdjustVideoView()
        {
            InitializeComponent();

            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Tick += new EventHandler(dispatcherTimer_Tick);
            _timer.Interval = new TimeSpan(0, 0, 1);
            
        }

        public DialogResult ShowDialogInt()
        {
            sldHorzDelay.Minimum = Scene.GetAdjustMin((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_HPRESET);
            sldHorzDelay.Maximum = Scene.GetAdjustMax((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_HPRESET);
            sldHorzDelay.Value = Scene.GetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_HPRESET);
            sldHorzDelay.TickFrequency = sldHorzDelay.Maximum / 20;

            sldVertDelay.Minimum = Scene.GetAdjustMin((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_VPRESET);
            sldVertDelay.Maximum = Scene.GetAdjustMax((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_VPRESET);
            sldVertDelay.Value = Scene.GetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_VPRESET);
            sldVertDelay.TickFrequency = sldVertDelay.Maximum / 20;
            
            //
            // Genlock Source
            //

            cboRefSource.Items.Add("Internal");
            cboRefSource.Items.Add("Ref In");
            cboRefSource.Items.Add("N/A");
            cboRefSource.Items.Add("Input 1");

            //
            // video Input
            //
            cboVideoInput.Items.Add("DISABLED");
            cboVideoInput.Items.Add("SDI");
            
            UpdateUI();

            _timer.Start();

            return System.Windows.Forms.DialogResult.OK;
        }

        private void sldHorzDelay_Scroll(object sender, EventArgs e)
        {
           Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_HPRESET, (int)sldHorzDelay.Value,0);
           UpdateUI();
       }

        private void UpdateUI()
        {
            sldHorzDelay.Value = Scene.GetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_HPRESET);
            sldVertDelay.Value = Scene.GetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_VPRESET);

            lblHDelay.Content = "H Delay: " + sldHorzDelay.Value.ToString();
            lblVDelay.Content = "V Delay: " + sldVertDelay.Value.ToString();

            int RefSource = Scene.GetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK);
            cboRefSource.SelectedIndex = RefSource;

            if (Scene.EnableInputVideo == false)
            {
                cboVideoInput.SelectedIndex = 0;
            }
            else
            {
                cboVideoInput.SelectedIndex = 1;
            }

            //
            // video Mixer
            //
            int path = Scene.GetAdjust((int)DSUITE_CP_TYPES.DSUITE_PATH);
            int colorbars = Scene.GetAdjust((int)DSUITE_CP_TYPES.DSUITE_OUTPUT_COLORBAR);

            if (colorbars != 0)
                rbtnColorBars.IsChecked = true;
            else if (path == (int)DSUITE_CP_PATH_TYPES.DSUITE_GRAPHIC_OVER_VIDEO)
                rbtnGraphicsOverVideo.IsChecked = true;
            else if (path == (int)DSUITE_CP_PATH_TYPES.DSUITE_GRAPHIC_ONLY)
                rbtnGraphicsOnly.IsChecked = true;
            else if (path == (int)DSUITE_CP_PATH_TYPES.DSUITE_VIDEO_ONLY)
                rbtnLiveVideoOnly.IsChecked = true;
        }

        private void sldVertDelay_Scroll(object sender, EventArgs e)
        {
            Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_VPRESET, (int)sldVertDelay.Value, 0);
            UpdateUI();
        }
          
        private void cboRefSource_SelectedIndexChanged(object sender, EventArgs e)
        {
           Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK,cboRefSource.SelectedIndex,0);
        }

        private void cboInputVideo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboVideoInput.SelectedIndex == 0)
            {
                Scene.EnableInputVideo = false;
            }
            else
            {
                Scene.EnableInputVideo = true;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (Scene.GetReferenceStatus() != 0)
            {
                lblRefStatus.Background = Brushes.Green;
                lblRefStatus.Content = "Valid";
            }
            else
            {
                lblRefStatus.Background = Brushes.Red;
                lblRefStatus.Content = "Invalid";
            }

            if (Scene.GetInputVideoStatus() != 0)
            {
                lblInputStatus.Background = Brushes.Green;
                lblInputStatus.Content = "Valid";
            }
            else
            {
                lblInputStatus.Background = Brushes.Red;
                lblInputStatus.Content = "Invalid";
            }

            int fronttemp = 0, backtemp = 0;
            //Scene.GetBoardTemp(ref fronttemp,ref backtemp);
            lblBoardTemperature.Content = "Board Temperature: " + fronttemp.ToString() + (char)186 + " C";      
            
        }

        private void rbtnGraphicsOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnGraphicsOnly.IsChecked==true)
                Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_PATH, (int)DSUITE_CP_PATH_TYPES.DSUITE_GRAPHIC_ONLY,0);
        }

        private void rbtnGraphicsOverVideo_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnGraphicsOverVideo.IsChecked == true)
                Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_PATH, (int)DSUITE_CP_PATH_TYPES.DSUITE_GRAPHIC_OVER_VIDEO, 0);

        }

        private void rbtnLiveVideoOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnLiveVideoOnly.IsChecked == true)
                Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_PATH, (int)DSUITE_CP_PATH_TYPES.DSUITE_SHAPED_GRAPHIC_OVER_VIDEO, 0);
        }

        private void rbtnColorBars_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnColorBars.IsChecked==true)    
                Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_OUTPUT_COLORBAR,1,0);
            else
                Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_OUTPUT_COLORBAR, 0, 0);

        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            int nval=0;
            //
            // These are the defaults from the board
            //
            Scene.GetAdjustNeutral((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_HPRESET, ref nval);
            Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_HPRESET,  nval, 0);

            Scene.GetAdjustNeutral((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_VPRESET, ref nval);
            Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK_VPRESET, nval, 0);
            
            //
            // These defaults are arbitrary, make up your own
            //
            Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_OUTPUT_COLORBAR, 0, 0);
            Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_PATH, (int)DSUITE_CP_PATH_TYPES.DSUITE_GRAPHIC_ONLY, 0);      
            Scene.SetAdjust((int)DSUITE_CP_TYPES.DSUITE_GENLOCK,(int)DSUITE_CP_GENLOCK_TYPES.DSUITE_GENLOCK_REFIN,0);        
            Scene.EnableInputVideo = false;

            UpdateUI();
        }
    }
}