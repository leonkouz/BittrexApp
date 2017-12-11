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
        /*public override View OnCreateView(LayoutInflater inflater,
            ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(
                Resource.Layout.Tab, container, false);

            return view;
        }*/

        //On creation of activity create the ListView and populate with all currencies from bittrex
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            List<MarketCurrency> currencies = APIMethods.GetCurrencies();
            List<string> currenciesStringList = new List<string>();

            foreach(var currency in currencies)
            {
                currenciesStringList.Add("BTC-" + currency.Currency);
            }

            this.ListAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleExpandableListItem1, currenciesStringList);
        }

        public override void OnListItemClick(ListView l, View v, int index, long id)
        {
            // We can display everything in place with fragments.
            // Have the list highlight this item and show the data.
            ListView.SetItemChecked(index, true);

            // Check what fragment is shown, replace if needed.
            var details = FragmentManager.FindFragmentById<DetailsFragment>(Resource.Id.details);
            if (details == null || details.ShownCurrency != index)
            {
                // Make new fragment to show this selection.
                details = DetailsFragment.NewInstance(index);

                // Execute a transaction, replacing any existing
                // fragment with this one inside the frame.
                var ft = FragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.details, details);
                ft.SetTransition(FragmentTransit.FragmentFade);
                ft.Commit();
            }
        }

    }

    class OrdersFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater,
            ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(
                Resource.Layout.ListViewTab, container, false);

            return view;
        }

        

    }

}

