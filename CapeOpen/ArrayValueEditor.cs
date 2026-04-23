using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace CapeOpen
{
    /// <summary>
    /// UITypeEditor for editing the array value of an <see cref="ArrayParameter"/>.
    /// Launches <see cref="ArrayValueEditorForm"/> to allow adding, removing, and editing
    /// elements of mixed types (double, int, bool, string).
    /// </summary>
    internal class ArrayValueEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider == null) return value;
            var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (svc == null) return value;

            object[] current = value as object[] ?? new object[0];
            using (var form = new ArrayValueEditorForm(current))
            {
                if (svc.ShowDialog(form) == DialogResult.OK)
                    return form.Result;
            }
            return value;
        }
    }

    /// <summary>
    /// Dialog form for editing the elements of an <see cref="ArrayParameter"/>.
    /// Supports mixed types: Double, Int32, Boolean, String.
    /// </summary>
    internal class ArrayValueEditorForm : Form
    {
        private DataGridView _grid;
        private Button _btnAdd;
        private Button _btnRemove;
        private Button _btnOk;
        private Button _btnCancel;
        private ComboBox _cboxType;

        public object[] Result { get; private set; }

        private static readonly string[] TypeNames = { "Double", "Int32", "Boolean", "String" };

        public ArrayValueEditorForm(object[] values)
        {
            Text = "数组元素编辑器";
            Width = 540;
            Height = 480;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            _grid = new DataGridView
            {
                Left = 10, Top = 10, Width = 500, Height = 340,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var colIndex = new DataGridViewTextBoxColumn { HeaderText = "#", ReadOnly = true, Width = 40, AutoSizeMode = DataGridViewAutoSizeColumnMode.None };
            var colType = new DataGridViewComboBoxColumn
            {
                HeaderText = "类型",
                Width = 100,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat
            };
            colType.Items.AddRange(TypeNames);
            var colValue = new DataGridViewTextBoxColumn { HeaderText = "值" };

            _grid.Columns.AddRange(new DataGridViewColumn[] { colIndex, colType, colValue });
            _grid.CellValueChanged += Grid_CellValueChanged;
            _grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            _cboxType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Left = 10, Top = 360, Width = 100
            };
            _cboxType.Items.AddRange(TypeNames);
            _cboxType.SelectedIndex = 0;

            _btnAdd = new Button { Text = "添加", Left = 120, Top = 360, Width = 75, Height = 28 };
            _btnAdd.Click += BtnAdd_Click;

            _btnRemove = new Button { Text = "删除", Left = 205, Top = 360, Width = 75, Height = 28 };
            _btnRemove.Click += BtnRemove_Click;

            _btnOk = new Button { Text = "确定", Left = 330, Top = 400, Width = 85, Height = 30, DialogResult = DialogResult.OK };
            _btnCancel = new Button { Text = "取消", Left = 425, Top = 400, Width = 85, Height = 30, DialogResult = DialogResult.Cancel };

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            Controls.AddRange(new Control[] { _grid, _cboxType, _btnAdd, _btnRemove, _btnOk, _btnCancel });

            // Populate initial values
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                    AddRow(values[i]);
            }

            _btnOk.Click += (s, e) =>
            {
                Result = BuildResult();
            };
        }

        private void AddRow(object val)
        {
            int idx = _grid.Rows.Count;
            string typeName = GetTypeName(val);
            string displayVal = val?.ToString() ?? "";
            if (val is bool b) displayVal = b ? "True" : "False";
            _grid.Rows.Add(idx.ToString(), typeName, displayVal);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string typeName = _cboxType.SelectedItem?.ToString() ?? "Double";
            object defaultVal = GetDefault(typeName);
            AddRow(defaultVal);
            RenumberRows();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in _grid.SelectedRows)
                {
                    if (!row.IsNewRow) _grid.Rows.Remove(row);
                }
                RenumberRows();
            }
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            // When user changes type column, reset value to default.
            if (e.ColumnIndex == 1)
            {
                string newType = _grid.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "Double";
                object def = GetDefault(newType);
                _grid.Rows[e.RowIndex].Cells[2].Value = def?.ToString() ?? "";
            }
        }

        private void RenumberRows()
        {
            for (int i = 0; i < _grid.Rows.Count; i++)
                _grid.Rows[i].Cells[0].Value = i.ToString();
        }

        private object[] BuildResult()
        {
            object[] result = new object[_grid.Rows.Count];
            for (int i = 0; i < _grid.Rows.Count; i++)
            {
                string typeName = _grid.Rows[i].Cells[1].Value?.ToString() ?? "Double";
                string valStr = _grid.Rows[i].Cells[2].Value?.ToString() ?? "";
                result[i] = ParseValue(typeName, valStr);
            }
            return result;
        }

        private static string GetTypeName(object val)
        {
            if (val is double) return "Double";
            if (val is int) return "Int32";
            if (val is bool) return "Boolean";
            if (val is string) return "String";
            return "Double";
        }

        private static object GetDefault(string typeName)
        {
            switch (typeName)
            {
                case "Double": return 0.0;
                case "Int32": return 0;
                case "Boolean": return false;
                case "String": return "";
                default: return 0.0;
            }
        }

        private static object ParseValue(string typeName, string valStr)
        {
            switch (typeName)
            {
                case "Double":
                    if (double.TryParse(valStr, out double d)) return d;
                    return 0.0;
                case "Int32":
                    if (int.TryParse(valStr, out int n)) return n;
                    return 0;
                case "Boolean":
                    if (bool.TryParse(valStr, out bool b)) return b;
                    return false;
                case "String":
                    return valStr;
                default:
                    return valStr;
            }
        }
    }
}
