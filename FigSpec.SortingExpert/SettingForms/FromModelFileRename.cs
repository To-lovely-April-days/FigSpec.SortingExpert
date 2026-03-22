using DevExpress.XtraEditors;
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
    public partial class FromModelFileRename : BaseFormSetting
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }

        public FromModelFileRename(string fullPath)
        {
            InitializeComponent();
            FullPath = fullPath;
            FileName = Path.GetFileNameWithoutExtension(fullPath);
        }

        private void FromModelRename_Load(object sender, EventArgs e)
        {
            txtName.Text = FileName;
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
                string directoryName = Path.GetDirectoryName(FullPath);
                
                if (FileName != name)
                {
                    string newFilePath = Path.Combine(directoryName, name + ".fsmodel");
                    string[] speFiles = Directory.GetFiles(directoryName, "*.fsmodel");
                    if (speFiles.Contains(newFilePath))
                    {
                        FormShowHelper.ShowMessage("名称重复".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                    File.Move(FullPath, newFilePath);
                    FullPath = newFilePath;
                    FileName = name;
                    DialogResult = DialogResult.OK;
                }

            }
            catch (Exception)
            {

            }
            finally
            {
                
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}