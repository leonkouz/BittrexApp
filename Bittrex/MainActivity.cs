using System;
using BittrexAPI;
using BittrexAPI.Structures;
using Newtonsoft.Json;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android;
using Android.Util;

namespace Bittrex
{

    [Activity(Label = "Bittrex")]
    public class MainActivity : Activity
    {

        List<MarketCurrency> currencies;
        List<string> currenciesStringList;
        ListView _listView;
        SearchableAdapter _adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {

            currencies = APIMethods.GetCurrencies();
            currenciesStringList = new List<string>();

            foreach (var currency in currencies)
            {
                if (currency.Currency == "FC2" || currency.Currency == "BTC" || currency.Currency == "SLG" || currency.Currency == "SFR" || currency.Currency == "NAUT")
                {
                    continue;
                }
                currenciesStringList.Add("BTC-" + currency.Currency);
            }

            Constants.ApiKey = LoginData.APIKey;
            Constants.SecretKey = LoginData.SecretKey;

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //Set the toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetActionBar(toolbar);
            ActionBar.Title = "Bittrex";

            //Initialise the adapter and search view
            _adapter = new SearchableAdapter(this, currenciesStringList);

            _listView = this.FindViewById<ListView>(Resource.Id.listView);
            _listView.Adapter = _adapter;

            _listView.ItemClick += _listView_ItemClick;

            var searchView = (SearchView)FindViewById(Resource.Id.searchView);

            var _searchView = searchView.JavaCast<SearchView>();

            _searchView.SetIconifiedByDefault(false);
            _searchView.SetBackgroundColor(Android.Graphics.Color.White);
            _searchView.SetQueryHint("Search currencies");

            _searchView.QueryTextChange += (s, e) => _adapter.Filter.InvokeFilter(new Java.Lang.String(e.NewText));

        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            if(item.ItemId == Resource.Id.menu_preferences)
            {
                StartActivity(new Android.Content.Intent(this, typeof(SettingsActivity)));
                return base.OnOptionsItemSelected(item);
            }

            Toast.MakeText(this, "Action selected: " + item.TitleFormatted,
                ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }

        void AddTab(string tabText, Fragment view)
        {
            var tab = this.ActionBar.NewTab();
            tab.SetText(tabText);

            // must set event handler before adding tab
            tab.TabSelected += delegate (object sender, ActionBar.TabEventArgs e)
            {
                var fragment = this.FragmentManager.FindFragmentById(Resource.Id.fragmentContainer);
                if (fragment != null)
                    e.FragmentTransaction.Remove(fragment);
                e.FragmentTransaction.Add(Resource.Id.fragmentContainer, view);
            };
            tab.TabUnselected += delegate (object sender, ActionBar.TabEventArgs e) {
                e.FragmentTransaction.Remove(view);
            };

            this.ActionBar.AddTab(tab);
        }

        private void _listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //Gets the text of the item selecxted
            var selectedItem = e.Parent.GetItemAtPosition(e.Position).ToString();

            //Creates a new fragment and parses the selected currency
            var fragment = CurrencyFragment.NewInstance(selectedItem);

            // Execute a transaction, replacing any existing fragment with this one inside the frame.
            var fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Add(Resource.Id.fragmentContainer, fragment);
            fragmentTransaction.AddToBackStack("main");
            fragmentTransaction.SetTransition(FragmentTransit.FragmentFade);
            fragmentTransaction.Commit();
        }
    }
    
}

