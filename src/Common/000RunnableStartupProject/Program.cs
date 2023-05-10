using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

//////////////////////////////////////////////////////////////////////////////////////
/// THIS 000 SOLUTION MUST HAVE "Project Dependencies" ON ALL OTHER TEST PROJECTS! ///
//////////////////////////////////////////////////////////////////////////////////////

//The purpose of this project in the solution is as follows:

//Here is a workaround for Snap-In Compiler not working right from batch files.


//Background:

//Crestron Simpl#Pro Snap-In Compiler can only build projects with the IDE editor
//open, as the Snap-In needs to access a COM object that the IDE editor initializes.

//Visual Studio 2008 command DEVENV /REBUILD executes without opening the IDE.

//But, Visual Studio 2008 command DEVENV /RUNEXIT builds while the IDE is open.

//A run-able start-up project is required to use a /RUNEXIT type of command line.


//So:

//1. Add a new project of type Windows, “Windows Forms Application” to solution.

//2. Double-click its form, and add into Load_Form() this line: Application.Exit();

//3. Make that project the “Start-up Project” (in Project, Set As Startup Project).

//5. Make it dependent upon all other projects (in Project, Project Dependencies).


//Here is part of a batch file invocation that we use to build nightly Simpl#Pro tests (two cases):

//rem A special requirement for SNAP-IN test projects is that each project must be a dependency of a runnable project,
//rem and in this command line, use /RunExit instead of /rebuild "Release" to cause the GUI to be opened during build.

//if "%ProgramFiles(x86)%"=="" (
//   echo "This is a 32 bit Windows, lacks (x86)"
//   "C:\Program Files\Microsoft Visual Studio 9.0\Common7\IDE\devenv.exe" SnapInTestProjects.sln /RunExit /Log _Log.txt /Out _Out.txt
//) else (
//   echo "This is a 64 bit Windows, has (x86)"
//   "C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE\devenv.exe" SnapInTestProjects.sln /RunExit /Log _Log.txt /Out _Out.txt
//)


namespace _000RunnableStartupProject
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
