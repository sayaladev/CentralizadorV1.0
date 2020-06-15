using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using TenTec.Windows.iGridLib;

namespace Centralizador.WinApp.GUI
{
	[System.ComponentModel.DefaultEvent("CellButtonClick")]
	public class IGButtonColumnManager
	{
		#region Definitons

		// Public constant to mark columns as button columns.
		public const string BUTTON_COLUMN_TAG = "button_column";

		// Parameters of cell buttons.
		private const int CELL_BUTTON_PADDING = 4;

		// The target grid.
		private iGrid fGrid;

		#region Event definitions

		public class IGCellButtonClickEventArgs : EventArgs
		{
			public readonly int RowIndex;
			public readonly int ColIndex;
			public IGCellButtonClickEventArgs(int rowIndex, int colIndex)
			{
				RowIndex = rowIndex;
				ColIndex = colIndex;
			}
		}
		public delegate void iGCellButtonClickEventHandler(object sender, IGCellButtonClickEventArgs e);
		public event iGCellButtonClickEventHandler CellButtonClick;

		public class IGCellButtonVisibleEventArgs : EventArgs
		{
			public readonly int RowIndex;
			public readonly int ColIndex;
			public bool ButtonVisible;
			public IGCellButtonVisibleEventArgs(int rowIndex, int colIndex, bool buttonVisible)
			{
				RowIndex = rowIndex;
				ColIndex = colIndex;
				ButtonVisible = buttonVisible;
			}
		}
		public delegate void iGCellButtonVisibleEventHandler(object sender, IGCellButtonVisibleEventArgs e);
		public event iGCellButtonVisibleEventHandler CellButtonVisible;

		public class IGCellButtonTooltipEventArgs : EventArgs
		{
			public readonly int RowIndex;
			public readonly int ColIndex;
			public string TooltipText;
			public IGCellButtonTooltipEventArgs(int rowIndex, int colIndex, string tooltipText)
			{
				RowIndex = rowIndex;
				ColIndex = colIndex;
				TooltipText = tooltipText;
			}
		}
		public delegate void iGCellButtonTooltipEventHandler(object sender, IGCellButtonTooltipEventArgs e);
		public event iGCellButtonTooltipEventHandler CellButtonTooltip;

		#endregion

		#endregion

		#region Attach method

		public void Attach(iGrid grid)
		{
			fGrid = grid;

			fGrid.RequestEdit += new iGRequestEditEventHandler(FGrid_RequestEdit);
			fGrid.KeyPress += new KeyPressEventHandler(FGrid_KeyPress);
			fGrid.CellClick += new iGCellClickEventHandler(FGrid_CellClick);
			fGrid.CellMouseDown += new iGCellMouseDownEventHandler(FGrid_CellMouseDown);
			fGrid.CellMouseUp += new iGCellMouseUpEventHandler(FGrid_CellMouseUp);
			fGrid.CellMouseEnter += new iGCellMouseEnterLeaveEventHandler(FGrid_CellMouseEnter);
			fGrid.CellMouseLeave += new iGCellMouseEnterLeaveEventHandler(FGrid_CellMouseLeave);
			fGrid.RequestCellToolTipText += new iGRequestCellToolTipTextEventHandler(FGrid_RequestCellToolTipText);
			fGrid.CustomDrawCellForeground += new iGCustomDrawCellEventHandler(FGrid_CustomDrawCellForeground);
			fGrid.CustomDrawCellGetHeight += new iGCustomDrawCellGetHeightEventHandler(FGrid_CustomDrawCellGetHeight);
			fGrid.CustomDrawCellGetWidth += new iGCustomDrawCellGetWidthEventHandler(FGrid_CustomDrawCellGetWidth);

			foreach (iGCol col in fGrid.Cols)
			{
				if (IsButtonColumn(col.Index))
				{
					col.CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;
				}			
			}
		}

		#endregion

		#region Overridable OnEvent methods

		protected virtual void OnCellButtonClick(IGCellButtonClickEventArgs e)
		{
			CellButtonClick?.Invoke(this, e);
		}

		protected virtual void OnCellButtonVisible(IGCellButtonVisibleEventArgs e)
		{
			CellButtonVisible?.Invoke(this, e);
		}

		protected virtual void OnCellButtonTooltip(IGCellButtonTooltipEventArgs e)
		{
			CellButtonTooltip?.Invoke(this, e);
		}

		#endregion

		#region Internal stuff

		private bool IsButtonColumn(int colIndex)
		{
			return ((string)fGrid.Cols[colIndex].Tag == BUTTON_COLUMN_TAG);
		}

		private bool IsCellButtonVisible(int rowIndex, int colIndex)
		{
			IGCellButtonVisibleEventArgs myCellButtonVisibleEventArgs = new IGCellButtonVisibleEventArgs(rowIndex, colIndex, true);
			OnCellButtonVisible(myCellButtonVisibleEventArgs);
			return myCellButtonVisibleEventArgs.ButtonVisible;
		}

		private bool IsCellButtonEnabled(int rowIndex, int colIndex)
		{
			return (fGrid.Cells[rowIndex, colIndex].Enabled != iGBool.False);
		}

		#endregion

		#region Target Grid Event Handlers

		#region Disable Editing of Button Cells

		// Disable the built-in iGrid text editor for button cells.
		private void FGrid_RequestEdit(object sender, iGRequestEditEventArgs e)
		{
			if (IsButtonColumn(e.ColIndex))
				e.DoDefault = false;
		}

		#endregion

		#region Cell Button Events

		// Keyboard interface: cell buttons can be clicked with the SPACE key.
		private void FGrid_KeyPress(object sender, KeyPressEventArgs e)
		{
			iGCell myCurCell = fGrid.CurCell;
			if ((e.KeyChar == ' ') && (myCurCell != null))
				if (IsCellButtonEnabled(myCurCell.RowIndex, myCurCell.ColIndex))
					OnCellButtonClick(new IGCellButtonClickEventArgs(myCurCell.RowIndex, myCurCell.ColIndex));
		}

		private void FGrid_CellClick(object sender, iGCellClickEventArgs e)
		{
			if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex) && IsCellButtonEnabled(e.RowIndex, e.ColIndex))
				OnCellButtonClick(new IGCellButtonClickEventArgs(e.RowIndex, e.ColIndex));
		}

		private void FGrid_CellMouseDown(object sender, iGCellMouseDownEventArgs e)
		{
			// Redraw the button cell in the pressed state.
			if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex) && IsCellButtonEnabled(e.RowIndex, e.ColIndex))
				fGrid.Invalidate(e.Bounds);
		}

		private void FGrid_CellMouseUp(object sender, iGCellMouseUpEventArgs e)
		{
			// Redraw the button cell in the non-pressed (normal) state.
			if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex) && IsCellButtonEnabled(e.RowIndex, e.ColIndex))
				fGrid.Invalidate(e.Bounds);
		}

		private void FGrid_CellMouseEnter(object sender, iGCellMouseEnterLeaveEventArgs e)
		{
			// Redraw the button cell in the hot state.
			if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex) && IsCellButtonEnabled(e.RowIndex, e.ColIndex))
				fGrid.Invalidate(e.Bounds);
		}

		private void FGrid_CellMouseLeave(object sender, iGCellMouseEnterLeaveEventArgs e)
		{
			// Redraw the button cell in the normal state.
			if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex) && IsCellButtonEnabled(e.RowIndex, e.ColIndex))
				fGrid.Invalidate(e.Bounds);
		}

		private void FGrid_RequestCellToolTipText(object sender, iGRequestCellToolTipTextEventArgs e)
		{
			if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex))
			{
				IGCellButtonTooltipEventArgs myCellButtonTooltipEventArgs = new IGCellButtonTooltipEventArgs(e.RowIndex, e.ColIndex, null);
				OnCellButtonTooltip(myCellButtonTooltipEventArgs);
				e.Text = myCellButtonTooltipEventArgs.TooltipText;
			}
		}

		#endregion

		#region Drawing Cell Button

		private void CenterRect(Rectangle objBig, ref Rectangle objSmall)
		{
			objSmall.X = (int)(objBig.X + Math.Round((objBig.Width / 2D) - (objSmall.Width / 2D), 0));
			objSmall.Y = (int)(objBig.Y + Math.Round((objBig.Height / 2D) - (objSmall.Height / 2D), 0));
		}

		private void FGrid_CustomDrawCellForeground(object sender, iGCustomDrawCellEventArgs e)
		{
			if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex))
			{
				iGCell myCell = fGrid.Cells[e.RowIndex, e.ColIndex];
				//myCell.ImageIndex = 6;
				// Determine the button state.
				PushButtonState myState;
				switch (e.State)
				{
					case iGControlState.Normal:
						myState = PushButtonState.Normal;
						break;
					case iGControlState.Hot:
						myState = PushButtonState.Hot;
						break;
					case iGControlState.Pressed:
						myState = PushButtonState.Pressed;
						break;
					default:
						myState = PushButtonState.Disabled;
						break;
				}

				// Draw a button cell.
				Image myImage = null;
				if ((fGrid.ImageList != null) && (myCell.ImageIndex > -1))
				{
					myImage = fGrid.ImageList.Images[myCell.ImageIndex];
				}
				if (myImage == null)
				{
					ButtonRenderer.DrawButton(e.Graphics, e.Bounds, myCell.Text, myCell.EffectiveFont, false, myState);
				}
				else
				{
					Rectangle myImageRect = new Rectangle(0, 0, myImage.Width, myImage.Height);
					CenterRect(e.Bounds, ref myImageRect);
					ButtonRenderer.DrawButton(e.Graphics, e.Bounds, null, myCell.EffectiveFont, myImage, myImageRect, false, myState);
				}
			}
		}

		private void FGrid_CustomDrawCellGetHeight(object sender, iGCustomDrawCellGetHeightEventArgs e)
		{
			// This event is raised while auto-adjusting the height of a row containing a custom draw cell.
			// Here we calculate the height of the cell's contents needed to display it entirely.
			if (e.RowIndex < 0)
			{
				// e.RowIndex equals -1 in the case when GetPrefferedRowHeight is called
				// (this is also done in the ISupportInitialize.EndInit call from the designer).
				e.Height = fGrid.Font.Height + CELL_BUTTON_PADDING * 2;
			}
			else
			{
				if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex))
				{
					iGCell myCell = fGrid.Cells[e.RowIndex, e.ColIndex];
					Image myImage = null;
					if ((fGrid.ImageList != null) && (myCell.ImageIndex > -1))
					{
						myImage = fGrid.ImageList.Images[myCell.ImageIndex];
					}
					if (myImage == null)
					{
						e.Height = fGrid.Cells[e.RowIndex, e.ColIndex].EffectiveFont.Height + CELL_BUTTON_PADDING * 2;
					}
					else
					{
						e.Height = myImage.Height + CELL_BUTTON_PADDING * 2;
					}
				}
			}
		}

		private void FGrid_CustomDrawCellGetWidth(object sender, iGCustomDrawCellGetWidthEventArgs e)
		{
			// This event is raised while auto-adjusting the width of a column containing a custom draw cell.
			// Here we calculate the width of the cell's contents needed to display it entirely.
			if (IsButtonColumn(e.ColIndex) && IsCellButtonVisible(e.RowIndex, e.ColIndex))
			{
				iGCell myCell = fGrid.Cells[e.RowIndex, e.ColIndex];
				Image myImage = null;
				if ((fGrid.ImageList != null) && (myCell.ImageIndex > -1))
				{
					myImage = fGrid.ImageList.Images[myCell.ImageIndex];
				}
				if (myImage == null)
				{
					using (Graphics myGraphics = fGrid.CreateGraphics())
						e.Width = (int)myGraphics.MeasureString(myCell.Text, myCell.EffectiveFont).Width + CELL_BUTTON_PADDING * 2;
				}
				else
				{
					e.Width = myImage.Width + CELL_BUTTON_PADDING * 2;
				}
			}
		}

		#endregion

		#endregion
	}
}
