using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BittrexAPI;
using BittrexAPI.Structures;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Bittrex
{
    class ToolbarOnClickListener : Java.Lang.Object, Toolbar.IOnMenuItemClickListener
    {
        TextView buyTextView;
        TextView sellTextView;
        TextView lastTextView;

        Activity activity;

        Ticker tick;

        string currencyString;

        public ToolbarOnClickListener(Activity Activity, TextView buy, TextView sell, TextView last, string currency)
        {
            activity = Activity;
            buyTextView = buy;
            sellTextView = sell;
            lastTextView = last;
            currencyString = currency;
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_refresh)
            {
                //Get API data for currency
                try
                {
                    tick = APIMethods.GetTicker(currencyString);
                    //Sets the buy and sell prices
                    buyTextView.Text = tick.Bid.ToString("0.#########");
                    sellTextView.Text = tick.Ask.ToString("0.#########");
                    lastTextView.Text = tick.Last.ToString("0.#########");

                    Toast.MakeText(activity, "Refreshed", ToastLength.Short).Show();
                    return true;
                }
                catch (Exception e)
                {
                    Toast.MakeText(activity, e.Message.ToString(), ToastLength.Short).Show();
                    return false;
                }
            }
            return false;
        }
    }
}