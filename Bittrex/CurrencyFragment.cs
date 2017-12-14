﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BittrexAPI;
using BittrexAPI.Structures;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Text;


namespace Bittrex
{
    public class CurrencyFragment : Fragment
    {
        public string currencyString { get { return Arguments.GetString("currency_string", ""); } }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here

            string text = this.GetText(this.Id);

        }


        public static CurrencyFragment NewInstance(string currency)
        {
            //Parses the currency to the fragment class
            var currencyFrag = new CurrencyFragment { Arguments = new Bundle() };
            currencyFrag.Arguments.PutString("currency_string", currency);
            return currencyFrag;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.CurrencyLayout, container, false);

            //Finds the text view
            TextView buyTextView = view.FindViewById<TextView>(Resource.Id.buyPrice);
            TextView sellTextView = view.FindViewById<TextView>(Resource.Id.sellPrice);

            //Get API data for currency
            Ticker tick = APIMethods.GetTicker(currencyString);

            //Sets the buy and sell prices
            buyTextView.Text = tick.Bid.ToString("0.#########");
            sellTextView.Text = tick.Ask.ToString("0.#########"); 

            return view;
        }
    }
}