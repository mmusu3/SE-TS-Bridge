namespace Installer;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        spaceEngineersLabel = new Label();
        spaceEngineersPathTextBox = new TextBox();
        browseSpaceEngineersButton = new Button();
        teamSpeakLabel = new Label();
        teamSpeakPathTextBox = new TextBox();
        browseTeamSpeakButton = new Button();
        installButton = new Button();
        cancelButton = new Button();
        SuspendLayout();
        // 
        // spaceEngineersLabel
        // 
        spaceEngineersLabel.AutoSize = true;
        spaceEngineersLabel.Location = new Point(12, 15);
        spaceEngineersLabel.Name = "spaceEngineersLabel";
        spaceEngineersLabel.Size = new Size(288, 15);
        spaceEngineersLabel.TabIndex = 0;
        spaceEngineersLabel.Text = "Space Engineers plugins folder or Pulsar install folder:";
        // 
        // spaceEngineersPathTextBox
        // 
        spaceEngineersPathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        spaceEngineersPathTextBox.Location = new Point(12, 36);
        spaceEngineersPathTextBox.Name = "spaceEngineersPathTextBox";
        spaceEngineersPathTextBox.Size = new Size(460, 23);
        spaceEngineersPathTextBox.TabIndex = 1;
        // 
        // browseSpaceEngineersButton
        // 
        browseSpaceEngineersButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        browseSpaceEngineersButton.Location = new Point(478, 35);
        browseSpaceEngineersButton.Name = "browseSpaceEngineersButton";
        browseSpaceEngineersButton.Size = new Size(90, 25);
        browseSpaceEngineersButton.TabIndex = 2;
        browseSpaceEngineersButton.Text = "Browse...";
        browseSpaceEngineersButton.UseVisualStyleBackColor = true;
        browseSpaceEngineersButton.Click += BrowseSpaceEngineersButton_Click;
        // 
        // teamSpeakLabel
        // 
        teamSpeakLabel.AutoSize = true;
        teamSpeakLabel.Location = new Point(12, 75);
        teamSpeakLabel.Name = "teamSpeakLabel";
        teamSpeakLabel.Size = new Size(146, 15);
        teamSpeakLabel.TabIndex = 3;
        teamSpeakLabel.Text = "TeamSpeak plugins folder:";
        // 
        // teamSpeakPathTextBox
        // 
        teamSpeakPathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        teamSpeakPathTextBox.Location = new Point(12, 96);
        teamSpeakPathTextBox.Name = "teamSpeakPathTextBox";
        teamSpeakPathTextBox.Size = new Size(460, 23);
        teamSpeakPathTextBox.TabIndex = 4;
        // 
        // browseTeamSpeakButton
        // 
        browseTeamSpeakButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        browseTeamSpeakButton.Location = new Point(478, 95);
        browseTeamSpeakButton.Name = "browseTeamSpeakButton";
        browseTeamSpeakButton.Size = new Size(90, 25);
        browseTeamSpeakButton.TabIndex = 5;
        browseTeamSpeakButton.Text = "Browse...";
        browseTeamSpeakButton.UseVisualStyleBackColor = true;
        browseTeamSpeakButton.Click += BrowseTeamSpeakButton_Click;
        // 
        // installButton
        // 
        installButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        installButton.Location = new Point(382, 140);
        installButton.Name = "installButton";
        installButton.Size = new Size(90, 28);
        installButton.TabIndex = 6;
        installButton.Text = "Install";
        installButton.UseVisualStyleBackColor = true;
        installButton.Click += InstallButton_Click;
        // 
        // cancelButton
        // 
        cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Location = new Point(478, 140);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(90, 28);
        cancelButton.TabIndex = 7;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Click += CancelButton_Click;
        // 
        // MainForm
        // 
        AcceptButton = installButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelButton;
        ClientSize = new Size(580, 180);
        Controls.Add(cancelButton);
        Controls.Add(installButton);
        Controls.Add(browseTeamSpeakButton);
        Controls.Add(teamSpeakPathTextBox);
        Controls.Add(teamSpeakLabel);
        Controls.Add(browseSpaceEngineersButton);
        Controls.Add(spaceEngineersPathTextBox);
        Controls.Add(spaceEngineersLabel);
        MinimumSize = new Size(450, 219);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SE-TS Bridge Installer";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label spaceEngineersLabel;
    private TextBox spaceEngineersPathTextBox;
    private Button browseSpaceEngineersButton;
    private Label teamSpeakLabel;
    private TextBox teamSpeakPathTextBox;
    private Button browseTeamSpeakButton;
    private Button installButton;
    private Button cancelButton;
}
