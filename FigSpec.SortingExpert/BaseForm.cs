using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
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

namespace FigSpec.SortingExpert
{
    public partial class BaseForm : XtraForm
    {

        public BaseForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载时执行
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            //加载多语言
            LoadLanguage(this);

            base.OnLoad(e);

        }

        /// <summary>
        /// 加载单个窗口的多语言
        /// </summary>
        /// <param name="form"></param>
        public static void LoadLanguage(Form form, bool isMust = false)
        {
            if (GlobalLanguage.LanguageLibrary?.Forms?.Count > 0)
            {
                if (GlobalLanguage.Language != EnumLanguage.zh_CN || isMust)
                {
                    FormItem formItem = GlobalLanguage.LanguageLibrary.Forms.Find(i => i.Key == form.Name);//页面翻译

                    if (formItem != null)
                    {
                        if (!string.IsNullOrEmpty(formItem.Value))
                            form.Text = formItem.Value;
                        LoadLanguage(form.Controls, formItem.Controls);
                    }
                }
            }
        }

        /// <summary>
        /// 加载页面语言
        /// </summary>
        /// <param name="sontrols"></param>
        /// <param name="items">页面翻译内容</param>
        /// <param name="paging">分页翻译内容</param>
        private static void LoadLanguage(Control.ControlCollection controls, List<Item> items)
        {
            //遍历窗体所有控件，
            foreach (Control control in controls)
            {
                var name = control.Name;
                if (control != null)
                {
                    if (control is Label || control is Button || control is SimpleButton || control is CheckBox
                        || control is RadioButton || control is GroupControl || control is LabelControl || control is GroupBox
                        || control is XtraTabPage || control is TabPage
                        || control is CheckEdit)
                    {
                        var item = items.Find(i => i.Key == control.Name);
                        if (item != null)
                        {
                            control.Text = item.Value;
                        }
                    }
                    //else if (control is DevExpress.XtraNavBar.NavBarControl barCtrl)
                    //{
                    //    foreach (DevExpress.XtraNavBar.NavBarGroup grpCtrl in barCtrl.Groups)
                    //    {
                    //        var item = items.Find(i => i.Key == grpCtrl.Name);
                    //        if (item != null)
                    //        {
                    //            grpCtrl.Caption = item.Value;
                    //        }

                    //    }
                    //    foreach (DevExpress.XtraNavBar.NavBarItem itemCtrl in barCtrl.Items)
                    //    {
                    //        var item = items.Find(i => i.Key == itemCtrl.Name);
                    //        if (item != null)
                    //        {
                    //            itemCtrl.Caption = item.Value;
                    //        }
                    //    }
                    //}
                    else if (control is DevExpress.XtraBars.Navigation.TileBar tileCtrl)
                    {
                        foreach (TileGroup group in tileCtrl.Groups)
                        {
                            foreach (TileItem tile in group.Items)
                            {
                                var item = items.Find(i => i.Key == tile.Name);
                                if (item != null)
                                {
                                    tile.Text = item.Value;
                                }
                            }
                        }
                    }
                    else if (control is TabNavigationPage)
                    {
                        var item = items.Find(i => i.Key == control.Name);
                        if (item != null)
                        {
                            TabNavigationPage c = control as TabNavigationPage;
                            c.Caption = item.Value;
                        }
                    }
                    else if (control is TreeList)
                    {
                        TreeList c = control as TreeList;
                        foreach (TreeListColumn column in c.Columns)
                        {
                            var item = items.Find(i => i.Key == column.Name);
                            if (item != null)
                            {
                                column.Caption = item.Value;
                            }
                        }
                    }
                    else if (control is GridControl)
                    {
                        GridControl c = control as GridControl;

                        //右键菜单翻译
                        if (c.ContextMenuStrip != null)
                        {
                            ContextMenuStrip cc = c.ContextMenuStrip;
                            foreach (ToolStripMenuItem tsmi in cc.Items)
                            {
                                var item = items.Find(i => i.Key == tsmi.Name);
                                if (item != null)
                                {
                                    tsmi.Text = item.Value;
                                }
                            }
                        }


                        GridView gv = c.DefaultView as GridView;
                        if (gv != null)
                        {
                            foreach (GridColumn gc in gv.Columns)
                            {
                                //列表中列标题翻译
                                var item = items.Find(i => i.Key == gc.Name);
                                if (item != null)
                                {
                                    gc.Caption = item.Value;
                                }

                                //列表中按钮名称翻译
                                RepositoryItem repositoryItem = gc.ColumnEdit;
                                if (repositoryItem != null && repositoryItem is RepositoryItem)
                                {
                                    RepositoryItemButtonEdit itemButton = repositoryItem as RepositoryItemButtonEdit;
                                    if (itemButton == null)
                                        continue;
                                    foreach (DevExpress.XtraEditors.Controls.EditorButton btn in itemButton.Buttons)
                                    {
                                        if (!string.IsNullOrEmpty(btn.Caption))
                                            btn.Caption = btn.Caption.ToMultiLanguage();
                                    }
                                }
                            }
                        }

                    }
                    else if (control is TabPane)
                    {
                        TabPane tp = control as TabPane;

                        foreach (TabNavigationPage page in tp.Pages)
                        {
                            var item = items.Find(i => i.Key == page.Name);
                            if (item != null)
                            {
                                page.Caption = item.Value;
                            }
                            LoadLanguage(page.Controls, items); //递归回调
                        }
                    }
                    else if (control is DevExpress.XtraBars.BarDockControl barDctrl)
                    {
                        if (barDctrl.Manager is null)
                            continue;
                        foreach (DevExpress.XtraBars.BarItem ctr in barDctrl.Manager.Items)
                        {
                            var item = items.Find(i => i.Key == ctr.Name);
                            if (item != null)
                            {
                                ctr.Caption = item.Value;
                            }
                        }
                        continue;
                    }
                    else if (control is UserControl)
                    {
                        FormItem formItem = GlobalLanguage.LanguageLibrary.Forms.Find(i => i.Key == control.Name);//页面翻译
                        if (formItem != null)
                        {
                            LoadLanguage(control.Controls, formItem.Controls); //递归回调
                        }
                    }
                    else if (control is DevExpress.XtraBars.Docking.DockPanel)
                    {
                        var item = items.Find(i => i.Key == control.Name);
                        if (item != null)
                        {
                            control.Text = item.Value;
                        }
                    }

                    if (control.HasChildren && !(control is UserControl))
                    {
                        LoadLanguage(control.Controls, items); //递归回调
                    }
                }
            }
        }

    }
}
