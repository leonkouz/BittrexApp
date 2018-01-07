using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Bittrex
{
    [Activity(Label = "CurrencyChartActivity")]
    public class CurrencyChartFragment : Fragment
    {

        public string CurrencyString { get { return Arguments.GetString("currency_string", ""); } }

        string pathUri;
        string filePath;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Reads the HTML file
            string contents;
            using (StreamReader sr = new StreamReader(MainActivity.assets.Open("tradingView.html")))
            {
                contents = sr.ReadToEnd();
            }

            //Gets the currency without the hyphen 
            string[] currencyArray = CurrencyString.Split('-');
            string currency = currencyArray[1] + currencyArray[0];

            string[] contentsArray = contents.Split(',');

            for (var i = 0; i < contentsArray.Length; i++)
            {
                contentsArray[i] += ",";
            }
            contentsArray[contentsArray.Length - 1] = contentsArray[contentsArray.Length - 1].Remove(contentsArray[contentsArray.Length - 1].Length - 1);

            string[] currencyToReplaceArray = contentsArray[1].Split('"');
            string currencyToReplace = currencyToReplaceArray[3];
            string existingCurrency = currencyToReplace.Substring(8, currencyToReplace.Length - 8);
            string newCurrency = currencyToReplace.Substring(8, currencyToReplace.Length - 8).Replace(existingCurrency, currency);

            string newCurrencyToReplace = currencyToReplace.Replace(existingCurrency, newCurrency);

            contentsArray[1] = contentsArray[1].Replace(currencyToReplace, newCurrencyToReplace);

            string newHTML = string.Concat(contentsArray);
            string html = newHTML.Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Replace(@"\", string.Empty);
                
            Regex trimmer = new Regex(@"\s\s+");

            html = trimmer.Replace(html, " ");
            
            string path = Android.OS.Environment.ExternalStorageDirectory.Path;
            filePath = Path.Combine(path, "tradingView.html");
            System.IO.File.WriteAllText(filePath, newHTML);

            var uri = Android.Net.Uri.Parse("file://" + filePath);
            pathUri = uri.ToString();
        }

        public static CurrencyChartFragment NewInstance(string currency)
        {


            //Parses the currency to the fragment class
            var currencyChartFrag = new CurrencyChartFragment { Arguments = new Bundle() };
            currencyChartFrag.Arguments.PutString("currency_string", currency);
            return currencyChartFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.CurrencyChartFragment, container, false);

            //Gets the bottom toolbar
            var bottomToolbar = (Toolbar)view.FindViewById(Resource.Id.currencyToolbarChartFragment);
            bottomToolbar.InflateMenu(Resource.Menu.bot_currency_menu);
            bottomToolbar.MenuItemClick += BottomToolbar_MenuItemClick;

            string contents;
            using (StreamReader sr = new StreamReader(filePath))
            {
                contents = sr.ReadToEnd();
            }

            //Finds and sets the webview
            Android.Webkit.WebView localWebView = view.FindViewById<Android.Webkit.WebView>(Resource.Id.chartWebView);
            localWebView.Settings.JavaScriptEnabled = true;
            localWebView.Settings.DomStorageEnabled = true;
            localWebView.Settings.BlockNetworkImage = false;
            localWebView.Settings.DisplayZoomControls = true;
            localWebView.Settings.LoadsImagesAutomatically = true;
            localWebView.LoadUrl(pathUri);

            return view;
        }

        private void BottomToolbar_MenuItemClick(object sender, Toolbar.MenuItemClickEventArgs e)
        {
            if (e.Item.ItemId == Resource.Id.currencyToolbar_ordersButton)
            {
                // Execute a transaction, replacing any existing fragment with this one inside the frame.
                var fragmentTransaction = FragmentManager.BeginTransaction();

                fragmentTransaction.Hide(this);

                //check if fragment has been added
                Fragment orderFragment = FragmentManager.FindFragmentByTag("orderFragment");

                if(orderFragment != null)
                {
                    if (orderFragment.IsAdded)
                    {
                        fragmentTransaction.Show(MainActivity.orderFragment);
                    }
                    else
                    {
                        fragmentTransaction.Add(Resource.Id.fragmentContainer, MainActivity.orderFragment, "orderFragment");
                    }
                }
                else
                {
                    fragmentTransaction.Add(Resource.Id.fragmentContainer, MainActivity.orderFragment, "orderFragment");
                }
                
                
                //fragmentTransaction.AddToBackStack(null);
                fragmentTransaction.SetTransition(FragmentTransit.FragmentFade);
                fragmentTransaction.Commit();
            }

            if (e.Item.ItemId == Resource.Id.currencyToolbar_chartButton)
            {
                return;
            }

            if (e.Item.ItemId == Resource.Id.currencyToolbar_historyButton)
            {
                return;
            }

        }
    }
}