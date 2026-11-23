using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace Installer;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        teamSpeakPathTextBox.Text = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TS3Client", "plugins");
    }

    void BrowseSpaceEngineersButton_Click(object sender, EventArgs e)
    {
        BrowseForFolder(spaceEngineersPathTextBox, "Select the Space Engineers plugin folder or Pulsar folder");
    }

    void BrowseTeamSpeakButton_Click(object sender, EventArgs e)
    {
        BrowseForFolder(teamSpeakPathTextBox, "Select the TeamSpeak plugins folder");
    }

    void CancelButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    void BrowseForFolder(TextBox textBox, string description)
    {
        try
        {
            var path = FolderPicker.Show(Handle, description, textBox.Text);

            if (path != null)
                textBox.Text = path;
        }
        catch (Exception ex) when (ex is COMException or IOException or UnauthorizedAccessException
            or SecurityException or ArgumentException or NotSupportedException or InvalidOperationException)
        {
            MessageBox.Show(this, $"Unable to select a folder.\n\n{ex.Message}",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void InstallButton_Click(object sender, EventArgs e)
    {
        try
        {
            var runningPrograms = PluginInstaller.Install(AppContext.BaseDirectory, spaceEngineersPathTextBox.Text, teamSpeakPathTextBox.Text);

            if (runningPrograms.Count != 0)
            {
                MessageBox.Show(this,
                    $"Close the following programs before installing: {string.Join(", ", runningPrograms)}.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show(this, "The plugins were installed successfully.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException
            or ArgumentException or NotSupportedException or InvalidOperationException or Win32Exception)
        {
            MessageBox.Show(this, $"Installation failed. The plugins may not have been fully installed.\n\n{ex.Message}",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
