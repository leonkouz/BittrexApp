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

        public bool isOnCurrencyFragment = false;

        IMenuItem refreshButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //Get the list of currencies
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
            //This is required to create the custom toolbar
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);

            refreshButton = menu.FindItem(Resource.Id.menu_refresh);
            refreshButton.SetVisible(false);
            refreshButton.SetEnabled(false);

            return base.OnCreateOptionsMenu(menu);
        }

        public override void OnBackPressed()
        {
            if (isOnCurrencyFragment == true)
            {
                refreshButton.SetVisible(false);
                refreshButton.SetEnabled(false);
                isOnCurrencyFragment = false;
            }
            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Resource.Id.menu_preferences)
            {
                StartActivity(new Android.Content.Intent(this, typeof(SettingsActivity)));
                return base.OnOptionsItemSelected(item);
            }

            return base.OnOptionsItemSelected(item);
        }

        private void _listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {

            refreshButton.SetEnabled(true);
            refreshButton.SetVisible(true);

            isOnCurrencyFragment = true;

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

