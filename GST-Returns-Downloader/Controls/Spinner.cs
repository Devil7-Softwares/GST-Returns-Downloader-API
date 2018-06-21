using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Devil7.Automation.GSTR.Downloader.Controls {
	internal class Spinner : UserControl {
		public Spinner () {
			InitializeComponent ();
		}

		private void InitializeComponent () {
			AvaloniaXamlLoader.Load (this);
		}
	}
}