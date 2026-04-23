// WinForms UI for CapeOpenRegistrar.
// Lets users add .dll assemblies, then Register / Unregister / Export-.reg
// on them. Also supports dumping a .tlb. All long-running work runs on a
// background thread and logs to the textbox via Invoke.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace CapeOpenRegistrar;

internal sealed class MainForm : Form
{
    readonly ListBox _list = new()
    {
        Dock = DockStyle.Fill,
        SelectionMode = SelectionMode.MultiExtended,
        IntegralHeight = false,
        AllowDrop = true,
        HorizontalScrollbar = true,
    };
    readonly TextBox _log = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = false,
        Font = new System.Drawing.Font("Consolas", 9f),
        BackColor = System.Drawing.Color.Black,
        ForeColor = System.Drawing.Color.Gainsboro,
    };
    readonly Button _btnAdd = new() { Text = "Add DLL...", AutoSize = true };
    readonly Button _btnRemove = new() { Text = "Remove", AutoSize = true };
    readonly Button _btnClear = new() { Text = "Clear", AutoSize = true };
    readonly Button _btnRegister = new() { Text = "Register", AutoSize = true, BackColor = System.Drawing.Color.LightGreen };
    readonly Button _btnUnregister = new() { Text = "Unregister", AutoSize = true, BackColor = System.Drawing.Color.MistyRose };
    readonly Button _btnExport = new() { Text = "Export .reg...", AutoSize = true };
    readonly Button _btnDumpTlb = new() { Text = "Dump .tlb...", AutoSize = true };
    readonly Button _btnClearLog = new() { Text = "Clear Log", AutoSize = true };
    readonly Button _btnElevate = new() { Text = "Relaunch as Administrator", AutoSize = true };
    readonly ComboBox _cboTarget = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 110,
    };
    readonly Label _adminLabel = new()
    {
        AutoSize = true,
        Padding = new Padding(6, 4, 6, 4),
    };

    public MainForm()
    {
        Text = "CapeOpen Registrar";
        Width = 1200;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new System.Drawing.Size(1200, 700);

        // Top bar: admin status
        bool isAdmin = IsRunningAsAdministrator();
        _adminLabel.Text = isAdmin
            ? "✔ Running as Administrator — HKLM writes allowed."
            : "⚠ NOT running as Administrator — Register/Unregister will fail. Click 'Relaunch as Administrator'.";
        _adminLabel.BackColor = isAdmin ? System.Drawing.Color.PaleGreen : System.Drawing.Color.LightYellow;
        _adminLabel.Dock = DockStyle.Top;
        _btnElevate.Visible = !isAdmin;

        // Action buttons (top)
        var topButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(4),
            WrapContents = false,
        };
        topButtons.Controls.AddRange(new Control[]
        {
            _btnAdd, _btnRemove, _btnClear,
            new Label { Text = "   ", AutoSize = true },
            new Label { Text = "Target:", AutoSize = true, Padding = new Padding(0, 6, 2, 0) },
            _cboTarget,
            _btnRegister, _btnUnregister, _btnExport,
            new Label { Text = "   ", AutoSize = true },
            _btnDumpTlb, _btnClearLog,
            new Label { Text = "   ", AutoSize = true },
            _btnElevate,
        });

        // List + log split
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 180,
        };
        var listGroup = new GroupBox { Text = "Managed assemblies (drag & drop supported)", Dock = DockStyle.Fill };
        listGroup.Controls.Add(_list);
        var logGroup = new GroupBox { Text = "Log", Dock = DockStyle.Fill };
        logGroup.Controls.Add(_log);
        split.Panel1.Controls.Add(listGroup);
        split.Panel2.Controls.Add(logGroup);

        Controls.Add(split);
        Controls.Add(topButtons);
        Controls.Add(_adminLabel);

        // Wire up
        _btnAdd.Click += (_, _) => AddFilesDialog();
        _btnRemove.Click += (_, _) => RemoveSelected();
        _btnClear.Click += (_, _) => _list.Items.Clear();
        _btnRegister.Click += async (_, _) => await RunAsync("register", "Register");
        _btnUnregister.Click += async (_, _) => await RunAsync("unregister", "Unregister");
        _btnExport.Click += async (_, _) => await ExportAsync();
        _btnDumpTlb.Click += async (_, _) => await DumpTlbAsync();
        _btnClearLog.Click += (_, _) => _log.Clear();
        _btnElevate.Click += (_, _) => RelaunchAsAdministrator();

        _list.DragEnter += OnListDragEnter;
        _list.DragDrop += OnListDragDrop;

        // Target dropdown: x86 writes to Wow6432Node (what 32-bit hosts
        // like PRO/II read); x64 writes to the native view; Both writes to both.
        _cboTarget.Items.AddRange(new object[] { "x86", "x64", "Both" });
        _cboTarget.SelectedIndex = 0;
    }

    Registrar.BitnessTarget SelectedTarget() => _cboTarget.SelectedIndex switch
    {
        0 => Registrar.BitnessTarget.X86,
        1 => Registrar.BitnessTarget.X64,
        2 => Registrar.BitnessTarget.Both,
        _ => Registrar.BitnessTarget.X86,
    };

    static bool IsRunningAsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch { return false; }
    }

    void RelaunchAsAdministrator()
    {
        try
        {
            var exe = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exe))
            {
                MessageBox.Show(this, "Could not determine current executable path.",
                    "Relaunch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Environment.CurrentDirectory,
            };
            Process.Start(psi);
            Close();
        }
        catch (Win32Exception)
        {
            // User cancelled UAC — ignore.
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Relaunch as Administrator",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void AddFilesDialog()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Managed assemblies (*.dll)|*.dll|All files (*.*)|*.*",
            Multiselect = true,
            Title = "Select CAPE-OPEN managed assembly",
        };
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            foreach (var f in ofd.FileNames)
                if (!_list.Items.Contains(f))
                    _list.Items.Add(f);
        }
    }

    void RemoveSelected()
    {
        var items = _list.SelectedItems.Cast<object>().ToArray();
        foreach (var i in items) _list.Items.Remove(i);
    }

    void OnListDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    void OnListDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
        {
            foreach (var f in files)
            {
                if (string.Equals(Path.GetExtension(f), ".dll", StringComparison.OrdinalIgnoreCase)
                    && !_list.Items.Contains(f))
                    _list.Items.Add(f);
            }
        }
    }

    string[] GetAssemblies() =>
        _list.Items.Cast<string>().ToArray();

    async Task RunAsync(string action, string niceName)
    {
        var asms = GetAssemblies();
        if (asms.Length == 0)
        {
            MessageBox.Show(this, "Add at least one managed .dll first.", niceName,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!IsRunningAsAdministrator())
        {
            if (MessageBox.Show(this,
                    "This operation writes to HKLM and requires Administrator.\n\n" +
                    "Proceed anyway (will likely fail)?",
                    niceName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
        }
        var target = SelectedTarget();
        await RunWorkAsync(niceName, log => Registrar.Run(action, asms, log, target));
    }

    async Task ExportAsync()
    {
        var asms = GetAssemblies();
        if (asms.Length == 0)
        {
            MessageBox.Show(this, "Add at least one managed .dll first.", "Export",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var sfd = new SaveFileDialog
        {
            Filter = "Registry file (*.reg)|*.reg",
            Title = "Save registration .reg as",
            FileName = "CapeOpen.reg",
        };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;
        var outReg = sfd.FileName;
        var target = SelectedTarget();
        await RunWorkAsync("Export", log => Registrar.Export(outReg, asms, log, target));
    }

    async Task DumpTlbAsync()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Type library (*.tlb)|*.tlb|All files (*.*)|*.*",
            Multiselect = true,
            Title = "Select .tlb to dump",
        };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;
        var tlbs = ofd.FileNames;
        await RunWorkAsync("Dump TLB", log => Registrar.DumpTlb(tlbs, log));
    }

    async Task RunWorkAsync(string name, Action<TextWriter> work)
    {
        SetBusy(true);
        AppendLog($"\n---- {name} started at {DateTime.Now:HH:mm:ss} ----\n");
        try
        {
            await Task.Run(() =>
            {
                var writer = new InvokingWriter(this);
                try { work(writer); }
                catch (Exception ex) { writer.WriteLine("ERROR: " + ex); }
            });
            AppendLog($"---- {name} finished at {DateTime.Now:HH:mm:ss} ----\n");
        }
        finally
        {
            SetBusy(false);
        }
    }

    void SetBusy(bool busy)
    {
        foreach (var b in new[] { _btnRegister, _btnUnregister, _btnExport, _btnDumpTlb, _btnAdd, _btnRemove, _btnClear })
            b.Enabled = !busy;
        UseWaitCursor = busy;
    }

    internal void AppendLog(string text)
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(AppendLog), text);
            return;
        }
        _log.AppendText(text);
    }

    // TextWriter that forwards to AppendLog on the UI thread.
    sealed class InvokingWriter : TextWriter
    {
        readonly MainForm _form;
        public InvokingWriter(MainForm form) { _form = form; }
        public override Encoding Encoding => Encoding.UTF8;
        public override void Write(char value) => _form.AppendLog(value.ToString());
        public override void Write(string? value) { if (value is not null) _form.AppendLog(value); }
        public override void WriteLine() => _form.AppendLog(Environment.NewLine);
        public override void WriteLine(string? value) => _form.AppendLog((value ?? string.Empty) + Environment.NewLine);
    }
}
