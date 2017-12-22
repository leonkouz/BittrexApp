﻿using System;
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
    class CurrencyOrderBookAdapter : BaseAdapter<Order>
    {
        private readonly IList<Order> _orders;
        private readonly Context _context;
        private readonly bool _isBuyOrders;


        public CurrencyOrderBookAdapter(Context context, IList<Order> orders, bool isBuyOrders)
        {
            _orders = orders;
            _context = context;
            _isBuyOrders = isBuyOrders; //Used to determine if it is the buy orders or the sell orders to set the colour of the text
        }

        public override Order this[int position]
        {
            get { return _orders[position]; }
        }

        public override int Count
        {
            get { return _orders.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var order = _orders[position];
            var view = convertView;

            if (view == null)
            {
                var inflater = LayoutInflater.FromContext(_context);
                view = inflater.Inflate(Resource.Layout.CurrencyOrderListViewItem, parent, false);
            }

            view.FindViewById<TextView>(Resource.Id.quantity).Text = order.Quantity.ToString("0.#########"); ;
            view.FindViewById<TextView>(Resource.Id.rate).Text = order.Rate.ToString("0.#########"); ;

            if (_isBuyOrders == true)
            {
                view.FindViewById<TextView>(Resource.Id.quantity).SetTextColor(Android.Graphics.Color.ForestGreen);
                view.FindViewById<TextView>(Resource.Id.rate).SetTextColor(Android.Graphics.Color.ForestGreen);
            }
            else
            {
                view.FindViewById<TextView>(Resource.Id.quantity).SetTextColor(Android.Graphics.Color.Red);
                view.FindViewById<TextView>(Resource.Id.rate).SetTextColor(Android.Graphics.Color.Red);
            }

            return view;

        }
    }
}