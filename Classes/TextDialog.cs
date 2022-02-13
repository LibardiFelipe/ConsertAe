using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsertAe.Classes
{
    public enum EMessageCode
    {
        Information,
        Question,
        Exclamation,
        Error
    };

    public static class TextDialog
    {
        public static System.Windows.Forms.DialogResult Show(string message,
            bool isYesOrNo = false,
            EMessageCode code = EMessageCode.Information,
            bool critical = false)
        {
            System.Windows.Forms.MessageBoxButtons buttons = isYesOrNo ? System.Windows.Forms.MessageBoxButtons.YesNo : System.Windows.Forms.MessageBoxButtons.OK;
            System.Windows.Forms.MessageBoxIcon icon;

            switch (code)
            {
                case EMessageCode.Information:
                    icon = System.Windows.Forms.MessageBoxIcon.Information;
                    break;
                case EMessageCode.Question:
                    icon = System.Windows.Forms.MessageBoxIcon.Question;
                    break;
                case EMessageCode.Exclamation:
                    icon = System.Windows.Forms.MessageBoxIcon.Exclamation;
                    break;
                default:
                    icon = System.Windows.Forms.MessageBoxIcon.Error;
                    break;
            }

            if (isYesOrNo)
                icon = System.Windows.Forms.MessageBoxIcon.Question;

            var result = System.Windows.Forms.MessageBox.Show(message,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                buttons, icon);

            if (critical)
                Environment.Exit(0);

            return result;
        }
    }
}
