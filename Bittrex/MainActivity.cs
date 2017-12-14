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

namespace Bittrex
{

    [Activity(Label = "Bittrex")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Constants.ApiKey = LoginData.APIKey;
            Constants.SecretKey = LoginData.SecretKey;

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            this.ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            AddTab("Currencies", new CurrenciesFragment());
            AddTab("Orders", new OrdersFragment());
            
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

        public override void OnBackPressed()
        {
            Fragment currentFragment = FragmentManager.FindFragmentById(Resource.Id.fragmentContainer);

            // Execute a transaction, replacing any existing fragment with this one inside the frame.
            var fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Remove(currentFragment);
            fragmentTransaction.AddToBackStack(null);
            fragmentTransaction.SetTransition(FragmentTransit.FragmentFade);
            fragmentTransaction.Commit();

        }


    }


    class CurrenciesFragment : Fragment
    {
        List<MarketCurrency> currencies;
        List<string> currenciesStringList;
        ListView _listView;

       

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.CustomListFragment, container, false);

            var _adapter = new SearchableAdapter(Activity, currenciesStringList);
            //var _adapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleExpandableListItem1, currenciesStringList);

            _listView = (ListView)view.FindViewById<ListView>(Resource.Id.listView);
            _listView.Adapter = _adapter;

            _listView.ItemClick += _listView_ItemClick;


            var searchView = (SearchView)view.FindViewById(Resource.Id.searchView);

            var _searchView = searchView.JavaCast<SearchView>();

            _searchView.QueryTextChange += (s, e) => _adapter.Filter.InvokeFilter(new Java.Lang.String(e.NewText));

            return view; 
        }

             
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            currencies = APIMethods.GetCurrencies();
            currenciesStringList = new List<string>();

            foreach (var currency in currencies)
            {
                
                if (currency.Currency == "FC2" || currency.Currency == "BTC" || currency.Currency == "SLG")
                {
                    continue;
                }
                currenciesStringList.Add("BTC-" + currency.Currency);
            }

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
            fragmentTransaction.AddToBackStack(null);
            fragmentTransaction.SetTransition(FragmentTransit.FragmentFade);
            fragmentTransaction.Commit();
        }

        
    }

    class OrdersFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater,
            ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(
                Resource.Layout.Main, container, false);

            return view;
        }
    }
}

