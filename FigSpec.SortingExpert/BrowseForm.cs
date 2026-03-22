using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.TypeExtensions;
using Globalization;
using Hyperspectral.SpectralFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert
{
    public partial class BrowseForm : BaseFormInside
    {
        private HypeImg curentHypeImg = null;

        private FileSystemWatcher watcher = new FileSystemWatcher();

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // 用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        public BrowseForm()
        {
            InitializeComponent();
            if (HypeImgData.hypeImgs == null)
            {
                HypeImgData.hypeImgs = HypeImgData.GetHypeImgRandoms(1);
            }
            var appset = GlobalSettings.ApplySetting;
            string path = appset.FolderPathOfBrowse;
            if (!Directory.Exists(path))
            {
                appset.FolderPathOfBrowse = path = GlobalSettings.ApplyInfo.ApplyImgPath;
                if (!Directory.Exists(GlobalSettings.ApplyInfo.ApplyImgPath))
                {
                    Directory.CreateDirectory(GlobalSettings.ApplyInfo.ApplyImgPath);
                }
                GlobalSettings.ApplySetting = appset;
            }
            if (GlobalSettings.ApplySetting.FolderPathOfBrowseRecords == null)
            {
                GlobalSettings.ApplySetting.FolderPathOfBrowseRecords = new List<string>() { path };
            }
        }

        private void BrowseForm_Load(object sender, EventArgs e)
        {
            var appset = GlobalSettings.ApplySetting;
            string path = appset.FolderPathOfBrowse;
            cboPath.Text = path;
            FolderWatcher(path);
            ChangeFolder(path);
            //RefreshPath();
            NotificationAction.FolderPathOfNewImgChanged = () =>
            {
                this.Invoke(new Action(() =>
                {
                    //if ((int)pg == 100)
                    //{
                    //    FormShowHelper.CloseLoadingForm();
                    //}
                    ChangeFolder(GlobalSettings.ApplySetting.FolderPathOfBrowse);
                    //RefreshPath();
                }));
               
            };
        }

        private void FolderWatcher(string path)
        {
            watcher.Path = path;

            watcher.NotifyFilter = NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            // Only watch text files.
            watcher.Filter = "*.spe";

            // Add event handlers.
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnChanged;

            // Start monitoring.
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (GlobalSettings.CurrentFormName == this.Name)
            {
                ChangeFolder(GlobalSettings.ApplySetting.FolderPathOfBrowse);
            }
        }
        private void ChangeFolder(string path)
        {
            watcher.Path = path;
            string[] speFiles = Directory.GetFiles(path, "*.spe");
            HypeImgData.hypeImgs.Clear();
            foreach (string pngFile in speFiles)
            {
                string pngpath = pngFile.ChangeExtension(".bmp");
                string hdrPath = pngFile.ChangeExtension(".hdr");
                if (File.Exists(pngpath))
                {
                    Image img = null;
                    using (Stream stream = File.Open(pngpath, FileMode.Open))
                    {
                        img = Image.FromStream(stream);
                    }
                    var m = new HypeImg()
                    {
                        image = img,
                        name = Path.GetFileNameWithoutExtension(pngpath),
                        fullPath = pngFile,
                        remark = "",
                    };
                    if (File.Exists(hdrPath))
                    {
                        HDR hdr = HDR.FromFile(hdrPath);
                        m.HdrInfo = hdr;
                    }
                    HypeImgData.hypeImgs.Add(m);
                }
            }

            if (!GlobalSettings.ApplySetting.FolderPathOfBrowseRecords.Exists(x => x == path))
            {
                GlobalSettings.ApplySetting.FolderPathOfBrowseRecords.Insert(0, path);
            }
            if (GlobalSettings.ApplySetting.FolderPathOfBrowseRecords.Count > 10)
            {
                GlobalSettings.ApplySetting.FolderPathOfBrowseRecords = GlobalSettings.ApplySetting.FolderPathOfBrowseRecords.GetRange(0, 1);
            }
            RefreshPath();
        }

        private void RefreshPath()
        {
            if (!this.IsHandleCreated)
                return;

            this.Invoke(new Action(() =>
            {
                cboPath.Properties.Items.Clear();
                cboPath.Properties.Items.AddRange(GlobalSettings.ApplySetting.FolderPathOfBrowseRecords);
                var settings = GlobalSettings.ApplySetting;
                cboPath.Text = settings.FolderPathOfBrowse;
                RefreshDataSource();
            }));
        }

        private string BrowseFolder(string path = "")
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择路径".ToMultiLanguage();
                dialog.ShowNewFolderButton = false;
                dialog.SelectedPath = path;
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }
            }
            return null;
        }


        private void tileView1_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            SetFocusedImg();
        }

        private void SetFocusedImg()
        {
            if (tileView1.FocusedRowHandle < 0)
            {
                curentHypeImg = null;
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
                txtFileInfo.Text = null;
                txtDeviceModel.Text = null;
                return;
            }
            HypeImg herodDto = tileView1.GetFocusedRow() as HypeImg;
            curentHypeImg = herodDto;
            pictureBox1.Image = herodDto.image;
            txtDeviceModel.Text = herodDto.HdrInfo?.FXModel;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("文件名：".ToMultiLanguage() + herodDto.name ?? string.Empty);
            builder.AppendLine("设备型号：".ToMultiLanguage() + herodDto.HdrInfo?.FXModel ?? string.Empty);
            builder.AppendLine("设备序列号：".ToMultiLanguage() + herodDto.HdrInfo?.SN ?? string.Empty);
            builder.AppendLine("曝光时间：".ToMultiLanguage() + (int)(herodDto.HdrInfo?.ExposureTime ?? 0) + "us");
            builder.AppendLine("增益：".ToMultiLanguage() + ((EnumGain)(int)(herodDto.HdrInfo?.Gain ?? 0)).ToMultiLanguage<EnumGain>());
            txtFileInfo.Text = builder.ToString();
        }

        private void BrowseForm_SizeChanged(object sender, EventArgs e)
        {
            panelControl3.Width = panelControl1.Width / 5 * 3;
            panelControl4.Location = new Point(panelControl3.Width + panelControl3.Location.X + 3, panelControl3.Location.Y);
            panelControl4.Width = panelControl1.Width - panelControl3.Width - panelControl3.Location.X - 5;
        }

        private void cboPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string path = cboPath.Text;
                if (StringExtensions.IsValidPath(path) && Directory.Exists(path))
                {
                    var settings = GlobalSettings.ApplySetting;
                    if (settings.FolderPathOfBrowse != path.Trim())
                    {
                        settings.FolderPathOfBrowse = path;
                        ChangeFolder(path);
                        GlobalSettings.ApplySetting = settings;
                        NotificationAction.FolderPathOfBrowseChanged?.Invoke();
                    }
                }
            }
        }

        private void cboPath_Leave(object sender, EventArgs e)
        {
            string path = GlobalSettings.ApplySetting.FolderPathOfBrowse;
            cboPath.Text = path;
        }

        private void btnSwitchPath_Click(object sender, EventArgs e)
        {
            var settings = GlobalSettings.ApplySetting;
            string path = BrowseFolder(settings.FolderPathOfBrowse);
            if (!string.IsNullOrEmpty(path))
            {
                settings.FolderPathOfBrowse = path;
                ChangeFolder(path);
                GlobalSettings.ApplySetting = settings;
                NotificationAction.FolderPathOfBrowseChanged?.Invoke();
            }
            else
            {

            }
        }

        private void cboPath_SelectedValueChanged(object sender, EventArgs e)
        {
            string path = cboPath.Text;
            if (StringExtensions.IsValidPath(path) && Directory.Exists(path))
            {
                var settings = GlobalSettings.ApplySetting;
                settings.FolderPathOfBrowse = path;
                ChangeFolder(path);
                GlobalSettings.ApplySetting = settings;
                NotificationAction.FolderPathOfBrowseChanged?.Invoke();
            }
        }


        private void btnSent2Trian_Click(object sender, EventArgs e)
        {
            if (curentHypeImg == null)
                return;
            NotificationAction.Send2Train?.Invoke(curentHypeImg.fullPath, true);
        }

        private void tileView1_ItemCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemCustomizeEventArgs e)
        {
            // 更改数据项的背景颜色为浅灰色
            e.Item.AppearanceItem.Normal.BackColor = Color.Gray;
            e.Item.AppearanceItem.Normal.BorderColor = Color.Gray;
            e.Item.AppearanceItem.Normal.ForeColor = Color.White;
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            RefreshDataSource();
        }

        private void RefreshDataSource(int rowHandle = -1)
        {
            string name = txtName.Text;
            if (!string.IsNullOrEmpty(name) && HypeImgData.hypeImgs.Count > 0)
            {
                var imgs = HypeImgData.hypeImgs.FindAll(o => o.name.Contains(name));
                if (imgs.Count > 0)
                {
                    gridControl1.DataSource = imgs;
                    rowHandle = rowHandle < imgs.Count ? rowHandle : -1;
                }

            }
            else
            {
                gridControl1.DataSource = HypeImgData.hypeImgs;
                rowHandle = rowHandle < HypeImgData.hypeImgs.Count ? rowHandle : -1;
            }
            tileView1.FocusedRowHandle = rowHandle;
            gridControl1.RefreshDataSource();
            SetFocusedImg();
        }

        private void barBtnDelete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (tileView1.FocusedRowHandle < 0)
                return;


            HypeImg herodDto = tileView1.GetFocusedRow() as HypeImg;
            if (herodDto == null)
                return;
            if (FormShowHelper.ShowMessage("将删除相关原始文件，确定删除吗？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) == DialogResult.OK)
            {
                int nextRowHandle = tileView1.FocusedRowHandle - 1;
                nextRowHandle = nextRowHandle < 0 ? 0 : nextRowHandle;

                watcher.EnableRaisingEvents = false;
                try
                {
                    string filePath = herodDto.fullPath;
                    string fileName = herodDto.name;
                    HypeImgData.hypeImgs.Remove(herodDto);
                    RefreshDataSource(nextRowHandle);

                    herodDto.image.Dispose();

                    string directoryPath = Path.GetDirectoryName(filePath);
                    string[] files = Directory.GetFiles(directoryPath);
                    foreach (var file in files)
                    {
                        if (Path.GetFileNameWithoutExtension(file) == fileName)
                        {
                            try
                            {
                                File.Delete(file);

                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    watcher.EnableRaisingEvents = true;
                }
            }
        }

        private void barBtnSend2Train_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (curentHypeImg == null)
                return;
            NotificationAction.Send2Train?.Invoke(curentHypeImg.fullPath, true);
        }

        private void tileView1_MouseUp(object sender, MouseEventArgs e)
        {
            var hitInfo = tileView1.CalcHitInfo(e.Location);
            if (hitInfo.InItem && e.Button == MouseButtons.Right)
            {
                popupMenu1.ShowPopup(MousePosition);
                SetFocusedImg();
            }
        }

        private void barBtnAppend2Train_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (curentHypeImg == null)
                return;
            string targetSpePath = GlobalSettings.ApplySetting.traingSet.spe?.SpePath;
            if (targetSpePath == curentHypeImg.fullPath)
            {
                FormShowHelper.ShowMessage("同一图片文件不可追加，请重新选择图片".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            if (string.IsNullOrEmpty(targetSpePath))
            {
                NotificationAction.Send2Train?.Invoke(curentHypeImg.fullPath, true);
            }
            else
            {
                
                var tempSpe = new SPE(curentHypeImg.fullPath);
                var targetSpe = GlobalSettings.ApplySetting.traingSet.spe;
                if (targetSpe.Hdr.DataType != tempSpe.Hdr.DataType ||
                    targetSpe.Hdr.Samples != tempSpe.Hdr.Samples || targetSpe.Hdr.Bands != tempSpe.Hdr.Bands)
                {
                    FormShowHelper.ShowMessage("追加的图片格式不匹配，请重新选择图片".ToMultiLanguage(), "提示".ToMultiLanguage());
                    tempSpe.Dispose();
                    return;
                }

                FormShowHelper.ShowLoadingForm(this, "数据复制中，请稍后".ToMultiLanguage() + "...");

                var tHdr = targetSpe.Hdr.Clone();
                string hdrPath = targetSpe.HdrPath;
                targetSpe.Dispose();
                GlobalSettings.ApplySetting.traingSet.spe = null;
                using (var sourceAccessor = tempSpe.Raw.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
                {
                    using (var destinationStream = new FileStream(targetSpePath, FileMode.Append, FileAccess.Write))
                    {
                        long fileSize = new FileInfo(curentHypeImg.fullPath).Length;

                        byte[] buffer = new byte[8192]; // 8KB buffer
                        long offset = 0;

                        while (offset < fileSize)
                        {
                            long remaining = fileSize - offset;
                            int sizeToRead = (int)Math.Min(buffer.Length, remaining);

                            sourceAccessor.ReadArray(offset, buffer, 0, sizeToRead);
                            destinationStream.Write(buffer, 0, sizeToRead);

                            offset += sizeToRead;
                        }
                    }
                }
                tHdr.Lines += tempSpe.Hdr.Lines;
                tHdr.Save(hdrPath);
                tempSpe.Dispose();

                try
                {
                    string pngpath = Path.ChangeExtension(targetSpePath, ".bmp");
                    if (File.Exists(pngpath))
                        File.Delete(pngpath);
                }
                catch
                {

                }
                
                NotificationAction.Send2Train?.Invoke(targetSpePath, false);
                FormShowHelper.CloseLoadingForm();
            }

        }
        /// <summary>
        /// 追加至训练模型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sortingBaseButton1_Click(object sender, EventArgs e)
        {
            barBtnAppend2Train_ItemClick(null, null);
        }
    }
}
