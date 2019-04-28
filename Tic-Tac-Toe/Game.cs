﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using TicTacToe.MouseEventsHandling;

namespace TicTacToe
{
	public partial class Game : Form
	{
		private int _playerNumber;
		private int _boardSize = 3;
		private readonly string[] _windowText = { Properties.Resources.Yin, Properties.Resources.Yang };
		private GraphicsPath _right, _upper, _down, _small, _small2;

		public Game()
		{
			InitializeComponent();
			StartGame();

			Application.AddMessageFilter(new MouseMessageHandler());
		}

		private void StartGame()
		{
			_playerNumber = 0;
			Text = _windowText[0];
			tableLayoutPanel.ColumnStyles.Clear();
			tableLayoutPanel.RowStyles.Clear();
			tableLayoutPanel.Controls.Clear();
			tableLayoutPanel.RowCount = _boardSize + 1;
			tableLayoutPanel.ColumnCount = _boardSize;

			for (int i = 0; i < tableLayoutPanel.RowCount - 1; ++i)
			{
				tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, (tableLayoutPanel.Height - trackBar.Height) / _boardSize));
				tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)100.0 / _boardSize));

				for (int j = 0; j < tableLayoutPanel.ColumnCount; ++j)
				{
					var btnCard = new Button
					{
						Tag = -1,
						Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
						BackColor = SystemColors.Control,
						Margin = new Padding(0)
					};
					btnCard.Click += pctrCard_Click;

					btnCard.ContextMenuStrip = new ContextMenuStrip();
					ToolStripMenuItem tsmi = new ToolStripMenuItem(Properties.Resources.Reset);
					tsmi.Owner = btnCard.ContextMenuStrip;
					btnCard.ContextMenuStrip.Items[0].Click += ResetCellValue;

					var gp = CreateDefaultEllipse();
					btnCard.Region = new Region(gp);
					btnCard.UseVisualStyleBackColor = true;
					tableLayoutPanel.Controls.Add(btnCard);
				}
			}
		}

		private void IsGameOver()
		{
			var winner = true;
			for (int i = 0; i < _boardSize; i++)
			{
				winner = true;
				Control con = tableLayoutPanel.GetControlFromPosition(i, 0);
				if ((int)con.Tag == -1)
					winner = false;
				for (int j = 0; j < _boardSize && winner; j++)
					if ((int)con.Tag != (int)tableLayoutPanel.GetControlFromPosition(i, j).Tag)
						winner = false;
				if (winner)
				{
					GameOver();
					return;
				}
			}
			for (int i = 0; i < _boardSize; i++)
			{
				winner = true;
				Control con = tableLayoutPanel.GetControlFromPosition(0, i);
				if ((int)con.Tag == -1)
					winner = false;
				for (int j = 0; j < _boardSize && winner; j++)
					if ((int)con.Tag != (int)tableLayoutPanel.GetControlFromPosition(j, i).Tag)
						winner = false;
				if (winner)
				{
					GameOver();
					return;
				}
			}
			for (int i = 1; i < _boardSize; i++)
			{
				winner = true;
				Control con = tableLayoutPanel.GetControlFromPosition(i, i);
				if ((int)con.Tag == -1)
				{
					winner = false;
					break;
				}
				if ((int)con.Tag != (int)tableLayoutPanel.GetControlFromPosition(0, 0).Tag)
				{
					winner = false;
					break;
				}
			}
			if (winner)
			{
				GameOver();
				return;
			}
			for (int i = _boardSize - 1; i >= 0; i--)
			{
				winner = true;
				Control con = tableLayoutPanel.GetControlFromPosition(_boardSize - i - 1, i);
				if ((int)con.Tag == -1)
				{
					winner = false;
					break;
				}
				if ((int)con.Tag != (int)tableLayoutPanel.GetControlFromPosition(0, _boardSize - 1).Tag)
				{
					winner = false;
					break;
				}
			}
			if (winner)
			{
				GameOver();
				return;
			}

			foreach (Button btnCard in tableLayoutPanel.Controls)
				if ((int)btnCard.Tag == -1)
				{
					return;
				}

			DialogResult dr = MessageBox.Show(Properties.Resources.Tie, null, MessageBoxButtons.YesNo);
			if (dr == DialogResult.Yes)
				StartGame();
			else
				Close();
		}

		private void GameOver()
		{
			DialogResult dr = MessageBox.Show(Properties.Resources.Win, null, MessageBoxButtons.YesNo);
			if (dr == DialogResult.Yes)
				StartGame();
			else
				Close();
		}

		private void ResetCellValue(object sender, EventArgs e)
		{
			ToolStripMenuItem c = sender as ToolStripMenuItem;
			ContextMenuStrip ts = (ContextMenuStrip)c.Owner;
			Control btn = ts.SourceControl;
			if ((int)btn.Tag != -1)
			{
				btn.Tag = -1;
				btn.Region = new Region(CreateDefaultEllipse());
			}
		}

		private void pctrCard_Click(Object sender, EventArgs e)
		{
			Control c = sender as Control;
			if ((int)c.Tag == -1)
			{
				var gp = CreateDefaultEllipse();
				ResetYinYangSegments();

				int x = c.Width / 35;
				int y = c.Height / 35;

				InitializeYinYangSegments(x, y, c.Size, _playerNumber);

				c.Region = CreateYinYangPath(gp);

				c.Tag = _playerNumber;
				_playerNumber = (_playerNumber + 1) % 2;
				Text = _windowText[_playerNumber];

				IsGameOver();
			}
		}

		private void BoardSizeChanged(object sender, EventArgs e)
		{
			_boardSize = trackBar.Value;
			StartGame();
		}

		private void tableLayoutPanel_SizeChanged(object sender, EventArgs e)
		{
			tableLayoutPanel.RowStyles.Clear();
			for (int i = 0; i < tableLayoutPanel.RowCount - 1; ++i)
				tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, (tableLayoutPanel.Height - trackBar.Height) / _boardSize));

			var gp = CreateDefaultEllipse();

			foreach (Button c in tableLayoutPanel.Controls)
			{
				if ((int)c.Tag != -1)
				{
					ResetYinYangSegments();

					int x = c.Width / 35;
					int y = c.Height / 35;

					InitializeYinYangSegments(x, y, c.Size, (int)c.Tag);

					c.Region = CreateYinYangPath(gp);

				}
				else
					c.Region = new Region(gp);
			}
		}

		private void InitializeYinYangSegments(int x, int y, Size size, int playerNumber)
		{
			var angle = 180;
			if (playerNumber == 1)
			{
				angle *= -1;
			}

			_right.AddArc(x, y, (tableLayoutPanel.Width - trackBar.Height / 2) / _boardSize - 2 * x, (tableLayoutPanel.Height - 3 * trackBar.Height / 2) / _boardSize - 2 * y, 90, angle);
			_upper.AddEllipse(size.Width * 1 / 5 + x, y, (size.Width - 3 * x) / 2, (size.Height - 3 * y) / 2);
			_down.AddEllipse(size.Width * 1 / 5 + x, size.Height / 2 - y, (size.Width - 3 * x) / 2, (size.Height - 3 * y) / 2);
			_small.AddEllipse(size.Width * 4 / 10 + x, size.Height * 2 / 9 + y, size.Width / 10, size.Height / 10);
			_small2.AddEllipse(size.Width * 4 / 10 + x, size.Height * 6 / 9 + y, size.Width / 10, size.Height / 10);
		}

		private Region CreateYinYangPath(GraphicsPath gp)
		{
			var yin = new Region(gp);
			yin.Exclude(_right);
			yin.Union(_upper);
			yin.Exclude(_down);
			yin.Exclude(_small);
			yin.Union(_small2);
			return yin;
		}

		private GraphicsPath CreateDefaultEllipse()
		{
			var gp = new GraphicsPath();
			gp.AddEllipse(0, 0, (tableLayoutPanel.Width - trackBar.Height / 2) / _boardSize + 3, (tableLayoutPanel.Height - 3 * trackBar.Height / 2) / _boardSize + 3);
			return gp;
		}

		private void ResetYinYangSegments()
		{
			_right = new GraphicsPath();
			_upper = new GraphicsPath();
			_small = new GraphicsPath();
			_down = new GraphicsPath();
			_small2 = new GraphicsPath();
		}
	}
}
