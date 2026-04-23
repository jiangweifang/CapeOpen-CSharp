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
}
