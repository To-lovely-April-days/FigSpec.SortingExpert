using DevExpress.XtraEditors.Controls;
using Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    /// <summary>
    /// Dev提示框按钮文本多语言
    /// </summary>
    public class MessageBoxExtend : Localizer
    {
        public override string GetLocalizedString(DevExpress.XtraEditors.Controls.StringId id)
        {
            switch (id)
            {
                case StringId.XtraMessageBoxCancelButtonText:
                    return "取消".ToMultiLanguage();
                case StringId.XtraMessageBoxOkButtonText:
                    return "确定".ToMultiLanguage();
                case StringId.XtraMessageBoxYesButtonText:
                    return "是".ToMultiLanguage();
                case StringId.XtraMessageBoxNoButtonText:
                    return "否".ToMultiLanguage();
                case StringId.XtraMessageBoxIgnoreButtonText:
                    return "忽略".ToMultiLanguage();
                case StringId.XtraMessageBoxAbortButtonText:
                    return "中止".ToMultiLanguage();
                case StringId.XtraMessageBoxRetryButtonText:
                    return "重试".ToMultiLanguage();
                default:
                    return base.GetLocalizedString(id);
            }
        }
    }
}
