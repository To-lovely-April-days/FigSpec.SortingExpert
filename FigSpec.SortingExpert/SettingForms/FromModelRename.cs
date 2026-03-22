using DevExpress.XtraEditors;
using Globalization;
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
    public partial class FromModelRename : BaseFormSetting
    {
        string uid;
        public string DisplayName { get; set; }
        public FromModelRename(string uid, string name)
        {
            InitializeComponent();
            txtName.Text = name;
            this.uid = uid;
        }

        private void FromModelRename_Load(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                FormShowHelper.ShowMessage("名称不能为空".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            if (GlobalSettings.ApplySetting.traingSet.models.Exists(i => i.uid != uid && i.name == name))
            {
                FormShowHelper.ShowMessage("名称重复".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }


            var set = GlobalSettings.ApplySetting.traingSet;
            var model = set.models.Find(i => i.uid == uid);
            DisplayName = model.name = name;
            GlobalSettings.ApplySetting.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}