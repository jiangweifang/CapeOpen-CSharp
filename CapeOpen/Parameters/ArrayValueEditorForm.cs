using System;
using System.Windows.Forms;

namespace CapeOpen
{
    /// <summary>
    /// Dialog form for editing the elements of an <see cref="ArrayParameter"/>.
    /// Supports mixed types: Double, Int32, Boolean, String.
    /// </summary>
    internal partial class ArrayValueEditorForm : Form
    {
        private static readonly string[] TypeNames = { "Double", "Int32", "Boolean", "String" };

        public object[] Result { get; private set; }

        public ArrayValueEditorForm(object[] values)
        {
            try
            {
                InitializeComponent();

                colType.Items.AddRange(TypeNames);
                cboxType.Items.AddRange(TypeNames);
                cboxType.SelectedIndex = 0;

                dataGridView1.CellValueChanged += Grid_CellValueChanged;
                dataGridView1.CurrentCellDirtyStateChanged += (s, e) =>
                {
                    if (dataGridView1.IsCurrentCellDirty)
                        dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
                };
                dataGridView1.DataError += (s, e) =>
                {
                    CrashLogger.LogException(e.Exception,
                        string.Format("ArrayValueEditorForm grid DataError row={0} col={1} context={2}",
                            e.RowIndex, e.ColumnIndex, e.Context));
                    e.ThrowException = false;
                };

                if (values != null)
                {
                    for (int i = 0; i < values.Length; i++)
                        AddRow(values[i]);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException(ex,
                    string.Format("ArrayValueEditorForm ctor failed (values.Length={0})",
                        values?.Length ?? -1));
                throw;
            }
        }

        private void AddRow(object val)
        {
            int idx = dataGridView1.Rows.Count;
            string typeName = GetTypeName(val);
            string displayVal = val?.ToString() ?? "";
            if (val is bool b) displayVal = b ? "True" : "False";
            dataGridView1.Rows.Add(idx.ToString(), typeName, displayVal);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string typeName = cboxType.SelectedItem?.ToString() ?? "Double";
            object defaultVal = GetDefault(typeName);
            AddRow(defaultVal);
            RenumberRows();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (!row.IsNewRow) dataGridView1.Rows.Remove(row);
                }
                RenumberRows();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Result = BuildResult();
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == 1)
            {
                string newType = dataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "Double";
                object def = GetDefault(newType);
                dataGridView1.Rows[e.RowIndex].Cells[2].Value = def?.ToString() ?? "";
            }
        }

        private void RenumberRows()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                dataGridView1.Rows[i].Cells[0].Value = i.ToString();
        }

        private object[] BuildResult()
        {
            object[] result = new object[dataGridView1.Rows.Count];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                string typeName = dataGridView1.Rows[i].Cells[1].Value?.ToString() ?? "Double";
                string valStr = dataGridView1.Rows[i].Cells[2].Value?.ToString() ?? "";
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
