using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // Get the path of the currently executing assembly
        string currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get the directory of the uninstaller
        string uninstallerDirectory = System.IO.Path.GetDirectoryName(currentAssemblyPath);

        // Launch the uninstaller process
        Process uninstallerProcess = Process.Start(currentAssemblyPath);

        // Wait for the uninstaller process to exit
        uninstallerProcess.WaitForExit();

        // Get the name of the program based on the uninstaller's filename
        string programName = System.IO.Path.GetFileNameWithoutExtension(currentAssemblyPath);

        // Generate the Control Panel command to uninstall the program
        string controlPanelCommand = $"appwiz.cpl,,2";

        // Start the Control Panel process with the uninstall command
        Process controlPanelProcess = Process.Start("control.exe", controlPanelCommand);

        // Wait for the Control Panel process to exit
        controlPanelProcess.WaitForExit();

        // Check if the program's folder is empty
        bool isFolderEmpty = System.IO.Directory.GetFiles(uninstallerDirectory).Length == 0;

        if (isFolderEmpty)
        {
            // Delete the program's folder
            System.IO.Directory.Delete(uninstallerDirectory);
        }

        Console.WriteLine("Program successfully uninstalled.");
    }
}