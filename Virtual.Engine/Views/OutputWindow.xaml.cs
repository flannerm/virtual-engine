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
using PageEngineLib;
using VideoToolkitLib;
using System.Configuration;

namespace Virtual.Engine.Views
{
    /// <summary>
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : Window
    {
        private bool _initialized = false;

        public OutputWindow()
        {
            InitializeComponent();
        }

        public void UpdateScene()
        {
            vxScene.Update();
        }
    }
}
