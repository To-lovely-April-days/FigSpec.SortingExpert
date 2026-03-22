using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class SortingSettingForm : BaseFormSetting
    {
        public Action<bool> updateSortingSetting;
        public SortingSettingForm()
        {
            InitializeComponent();

        }

        private void checkOriImg_CheckedChanged(object sender, EventArgs e)
        {
            //updateSortingSetting?.Invoke(checkOriImg.Checked);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {

        }
    }
}
