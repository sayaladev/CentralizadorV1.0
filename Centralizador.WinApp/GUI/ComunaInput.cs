using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Centralizador.Models.DataBase;

namespace Centralizador.WinApp.GUI
{
    internal class ComunaInput
    {
        public static IList<Comuna> Comunas { get; set; }
        public static TextBox TextBox  { get; set; }
        public static string ShowDialog(string title, string promptText, string rzn, string rut, string add, IList<Comuna> comunas)
        {
            Comunas = comunas;
            Form form = new Form();
            Label label = new Label();
            TextBox = new TextBox();
            TextBox.TextChanged += TextBox_TextChanged;
            TextBox.KeyPress += TextBox_KeyPress;
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(promptText);
            builder.AppendLine("");
            builder.AppendLine($"Name: {rzn}");
            builder.AppendLine($"Rut: {rut}");
            builder.AppendLine($"Address: {add}");

            form.Text = title;
            label.Text = builder.ToString();

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;
            // Left - Top - Ancho - Alto
            label.SetBounds(9, 10, 372, 13);
            TextBox.SetBounds(12, 80, 372, 20);
            buttonOk.SetBounds(228, 120, 80, 23);
            buttonCancel.SetBounds(309, 120, 80, 23);

            label.AutoSize = true;
            TextBox.Anchor = TextBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            // Ancho, alto
            form.ClientSize = new Size(396, 150);
            form.Controls.AddRange(new Control[] { label, TextBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            return TextBox.Text;

            //return dialogResult;
        }

        private static void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\b' && TextBox.Text.Length > 0)
            {
                TextBox.Text = "";
            }
        }

        private static void TextBox_TextChanged(object sender, EventArgs e)
        {
            var txt = sender as TextBox;
         
            if (Comunas != null && txt.Text.Length > 4)
            {
                Comuna res = Comunas.FirstOrDefault(c => c.ComDes.Contains(txt.Text));
                if (res != null)
                {
                    TextBox.Text = res.ComDes;
                }

            }
        }
    }
}
