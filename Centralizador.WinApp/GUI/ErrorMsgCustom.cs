using System;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace Centralizador.WinApp.GUI
{

    [Serializable()]
    public class ErrorMsgCen : Exception
    {

        // Constructors
        public ErrorMsgCen() : base()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="msgIcon"></param>
        public ErrorMsgCen(string message, MessageBoxIcon msgIcon) : base(message)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Error: " + message);
            builder.AppendLine("");
            MessageBox.Show(builder.ToString(), Application.ProductName, MessageBoxButtons.OK, msgIcon);
        }
        public ErrorMsgCen(string msgTitle, string msgDetail, MessageBoxIcon msgIcon) : base(msgTitle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Error: " + msgTitle);
            builder.AppendLine("");
            builder.AppendLine(msgDetail);
            MessageBox.Show(builder.ToString(), Application.ProductName, MessageBoxButtons.OK, msgIcon);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="msgIcon"></param>
        public ErrorMsgCen(string msgTitle, Exception innerException, MessageBoxIcon msgIcon) :
           base(msgTitle, innerException)
        {
            // Add any type-specific logic for inner exceptions.
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Error: " + msgTitle);
            builder.AppendLine("");
            if (innerException.InnerException != null)
            {
                builder.AppendLine(innerException.InnerException.Message);
            }
            else
            {
                builder.AppendLine(innerException.Message);
            }

            MessageBox.Show(builder.ToString(), Application.ProductName, MessageBoxButtons.OK, msgIcon);
        }
        protected ErrorMsgCen(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }

    }
}

