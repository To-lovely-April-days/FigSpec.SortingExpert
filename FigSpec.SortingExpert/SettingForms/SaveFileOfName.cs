using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class SaveFileOfName : BaseFormSetting
    {
        public string FileName { get; set; }
        public SaveFileOfName()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtName.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    FormShowHelper.ShowMessage("名称不能为空".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                var appset = GlobalSettings.ApplySetting;
                string path = appset.FolderPathOfBrowse;
                {
                    string newFilePath = Path.Combine(path, name + ".hdr");
                    string[] speFiles = Directory.GetFiles(path, "*.hdr");
                    if (speFiles.Contains(newFilePath))
                    {
                        FormShowHelper.ShowMessage("名称重复".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                    FileName = name;
                    DialogResult = DialogResult.OK;
                    Close();
                }

            }
            catch (Exception)
            {

            }
            finally
            {
           
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
