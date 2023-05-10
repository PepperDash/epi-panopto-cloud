using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//////////////////////////////////////////////////////////////////////////////////////
/// THIS 000 SOLUTION MUST HAVE "Project Dependencies" ON ALL OTHER TEST PROJECTS! ///
//////////////////////////////////////////////////////////////////////////////////////

namespace _000RunnableStartupProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // In order to build "Crestron Snap-In Compiler" types of projects,
            // DEVENV /Run must be used to cause IDE to be up during the build.
            //
            // This project is specified as the Solution's one Start-Up Project
            // so that DEVENV /Run will not show a fail dialog after the build.
            //
            // This project shall also list all other projects as dependencies,
            // so that they will be build on the way to building/running this.
            //
            // Hmmm. A console Application ends up with a Command Prompt open.
            // I guess I need to make a GUI app instead, which then can exit.

            Application.Exit();
        }
    }
}
