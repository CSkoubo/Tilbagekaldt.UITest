using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace TestCloudTest
{
	[TestFixture]
	public class Tests
	{
		AndroidApp app;

		[SetUp]
		public void BeforeEachTest ()
		{
			app = ConfigureApp.Android.ApkFile("C:/Users/cmskoubo/desktop/free.crapFoodNotifyMe.Android.apk").StartApp();
		}

		[Test]
		public void RunRepl()
		{
			app.Repl();
		}

		[Test]
		public void AppLaunches ()
		{
			//I am waiting for the ListView element to appear. You can add timeouts etc, if your app is a long time to load or doing async in the background.
			app.WaitForElement(c=> c.Class("ListView"));
			//I have put it into a method as im trying to keep my code DRY (Don't-Repeat-Yourself)
			IsNewestScreenPresented ();
			//Hey! Lets take a screenshot! So we can see what the app looks like.
			app.Screenshot("AppStarted");
		}

		public void IsNewestScreenPresented()
		{			
			app.WaitForElement(c => c.Class ("FormsTextView").Text("Sidste 7 dage:"));
			//Query for TextView where the Text property is as below
			var newestWithDraw = app.Query(c=>c.Class("TextView").Text("Nyeste tilbagetrækninger"));
			//I would only expect 1 TextView, if there are more its a bug.
			Assert.AreEqual(1, newestWithDraw.Count());
		}

		[Test]
		public void EnsureMenuCorrect()
		{			
			//Make sure we are on start screen.
			AppLaunches ();
			//Method for opening the drawer
			OpenDrawer ();
			//Get the FormsTextView.Text values into a list
			var menuItemsStringList = app.Query(c=>c.Descendant("ListView")
				.Index(1)
				.Class("FormsTextView"))
				.Select(x=>x.Text).ToList();
			//Make sure the two lists are equal
			Assert.AreEqual(new string[]{ "Tilbagekaldte fødevarer", "Nyeste", "Historik", "Smiley ordning", "Rapporter", "Maps" }, menuItemsStringList);

		}

		public void OpenDrawer()
		{
			//Tap the up menu button.
			app.Tap(c=>c.Marked("up"));
		}

		[Test]
		public void TestNewestAvailableWarning()
		{
			AppLaunches ();
			IsNewestScreenPresented ();
			TapServiceWarningListView ();
			IsCustomBannerEnabledOnServiceWarningPage ();
			TapOpenLinkButton ();
		}

		public void TapServiceWarningListView(int index = 1)
		{
			//Tap the second view in the ListView, as i know the first one is being used for the listview title.
			app.Tap(c=>c.Descendant("ListView").Descendant("View").Index(index));
			app.Screenshot("Tapped on the first element");
		}

		public void IsCustomBannerEnabledOnServiceWarningPage()
		{
			app.ScrollDown();
			//Search for my Admob banner, lets have a timeout just in case.
			app.WaitForElement (s => s.Class ("CustomBannerRenderer"), "Time out waiting for customBanner", new TimeSpan (5000));
			//Is banner enabled?
			var customBannerEnabled = app.Query(c=>c.Class("CustomBannerRenderer").Property("Enabled").Value<bool>());
			Assert.IsTrue (customBannerEnabled[0]);
		}

		public void TapOpenLinkButton()
		{
			app.Tap (c => c.Button());
		}

		[Test]
		public void HistoryAvailableWarnings()
		{
			AppLaunches();
			OpenDrawerTapTextView ("Historik");
			//Making sure we are on the right screen, using the title as identification.
			var newestWithDraw = app.Query(c=>c.Class("TextView").Text("Historik"));
			Assert.GreaterOrEqual(1, newestWithDraw.Count());

			TapServiceWarningListView ();
			IsCustomBannerEnabledOnServiceWarningPage ();
			TapOpenLinkButton ();
		}

		public void OpenDrawerTapTextView(string menuName)
		{
			OpenDrawer ();
			app.Tap(c=>c.Descendant("ListView")
				.Index(1)
				.Class("FormsTextView")
				.Text(menuName));
		}
	}
}

