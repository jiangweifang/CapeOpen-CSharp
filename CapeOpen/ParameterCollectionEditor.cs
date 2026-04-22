using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapeOpen
{
    internal class ParameterCollectionEditor : CollectionEditor
    {
        private ComboBox _comboBox;
        public ParameterCollectionEditor(Type type) : base(type)
        {
        }
        protected override object CreateInstance(Type itemType)
        {
            var types = new[] { typeof(RealParameter), typeof(IntegerParameter), typeof(BooleanParameter), typeof(OptionParameter), typeof(ArrayParameter) };
            var selectedType = _comboBox != null && _comboBox.SelectedIndex >= 0
                ? types[_comboBox.SelectedIndex]
                : typeof(RealParameter);

            if (selectedType == typeof(RealParameter))
                return new RealParameter("新建实数参数", "描述", 0.0, 0.0, 0.0, 100.0, CapeParamMode.CAPE_INPUT, "Pa");
            if (selectedType == typeof(IntegerParameter))
                return new IntegerParameter("新建整数参数", "描述", 0, 0, 0, 100, CapeParamMode.CAPE_INPUT_OUTPUT);
            if (selectedType == typeof(BooleanParameter))
                return new BooleanParameter("新建布尔参数", "描述", false, false, CapeParamMode.CAPE_INPUT_OUTPUT);
            if (selectedType == typeof(OptionParameter))
                return new OptionParameter("新建选项参数", "描述", "选项1", "选项1", new[] { "选项1", "选项2" }, true, CapeParamMode.CAPE_INPUT_OUTPUT);
            if (selectedType == typeof(ArrayParameter))
                return new ArrayParameter("新建数组参数", "描述", new object[] { 0.0, 0.0, 0.0 }, new object[] { 0.0, 0.0, 0.0 }, CapeParamMode.CAPE_INPUT_OUTPUT);

            return null;
        }

        protected override CollectionForm CreateCollectionForm()
        {
            var form = base.CreateCollectionForm();
            foreach (Control c in form.Controls)
            {
                c.Top += 30;
                c.Height -= 30;
            }

            _comboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cboxParamType",
                Left = 30,
                Top = 0,
                Width = 200
            };
            var types = new[]
            {
                typeof(RealParameter),
                typeof(IntegerParameter),
                typeof(BooleanParameter),
                typeof(OptionParameter),
                typeof(ArrayParameter)
            };
            _comboBox.Items.AddRange(types.Select(t => t.Name).ToArray());
            _comboBox.SelectedIndex = 0;
            form.Controls.Add(_comboBox);

            return form;
        }
    }
}
