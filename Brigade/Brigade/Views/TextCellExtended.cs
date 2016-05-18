using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Brigade.Views
{
    public class TextCellExtended : TextCell
    {
		public static readonly BindableProperty ShowDisclosureProperty = BindableProperty.Create<TextCellExtended, bool>(p => p.ShowDisclosure, default(bool));

		public bool ShowDisclosure
		{
			get { return (bool)GetValue(ShowDisclosureProperty); }
			set { SetValue(ShowDisclosureProperty, value); }
		}
	}
}
