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

    }


    class CurrenciesFragment : ListFragment
    {
        List<MarketCurrency> currencies;
        List<string> currenciesStringList;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.CustomListFragment, container, false);

            return view; 
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {

            IMenuItem searchMenuItem = menu.FindItem(Resource.Id.searchView); // get my MenuItem with placeholder submenu
            searchMenuItem.ExpandActionView();
        }

        //On creation of activity create the ListView and populate with all currencies from bittrex
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            currencies = APIMethods.GetCurrencies();
            currenciesStringList = new List<string>();

            foreach (var currency in currencies)
            {
                //Ignore this coin because its fucked
                if(currency.Currency == "FC2" || currency.Currency == "BTC" || currency.Currency == "SLG")
                {
                    continue;
                }
                currenciesStringList.Add("BTC-" + currency.Currency);
            }

            this.ListAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleExpandableListItem1, currenciesStringList);
        }

        public override void OnListItemClick(ListView l, View v, int index, long id)
        {
            //Gets the text of the item selecxted
            var selectedItem = l.GetItemAtPosition(index).ToString();

            //Creates a new fragment and parses the selected currency
            var fragment = CurrencyFragment.NewInstance(selectedItem);

            // Execute a transaction, replacing any existing fragment with this one inside the frame.
            var fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.fragmentContainer, fragment);
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

