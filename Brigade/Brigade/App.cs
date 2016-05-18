using Brigade.Core;
using Xamarin.Forms;

namespace Brigade
{
	public class App : Application
	{
		public App()
		{
			(new Bootstrapper(this)).Run();
		}

		protected override void OnStart()
		{
			base.OnStart();
		}

		// The root page of your application
		//MainPage = new ContentPage {
		//	Content = new StackLayout {
		//		VerticalOptions = LayoutOptions.Center,
		//		Children = {
		//			new Label {
		//				XAlign = TextAlignment.Start,
		//				Text = "Welcome to Xamarin Forms!"
		//			},
		//			new Button { Text = "Assets" },
		//			new Button { Text = "Availability" },
		//			new Button { Text = "Events" },
		//			new Button { Text = "Tasks & Reminders" },
		//			new Button { Text = "Admin" }
		//		}
		//	}
		//};
	}

}
