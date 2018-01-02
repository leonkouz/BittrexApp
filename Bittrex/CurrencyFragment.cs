using System;
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
using System.Threading.Tasks;

namespace Bittrex
{
    public class CurrencyFragment : Fragment
    {

        public string currencyString { get { return Arguments.GetString("currency_string", ""); } }

        public static string Currency;

        public static TextView buyTextView;
        public static TextView sellTextView;
        public static TextView lastTextView;
        public static Ticker tick;

        public static TextView btcBalance;
        public static TextView selectedCurrencyBalance;

        public static EditText orderAmount;
        public static EditText orderPrice;
        public static TextView totalPriceBtc;

        OrderBook orderBook;

        ListView buyOrderListView;
        ListView sellOrderListView;
        CurrencyOrderBookAdapter buyAdapter;
        CurrencyOrderBookAdapter sellAdapter;

        public static System.Timers.Timer timer;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }


        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public static CurrencyFragment NewInstance(string currency)
        {
            //Parses the currency to the fragment class
            var currencyFrag = new CurrencyFragment { Arguments = new Bundle() };
            currencyFrag.Arguments.PutString("currency_string", currency);
            Currency = currency;
            return currencyFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.CurrencyLayout, container, false);

            var viewMain = inflater.Inflate(Resource.Layout.Main, null, false);

            var toolbar = viewMain.FindViewById<Toolbar>(Resource.Id.toolbar);

            //Hooks into the refresh button listerner
            ToolbarOnClickListener listener = new ToolbarOnClickListener(Activity, buyTextView, sellTextView, lastTextView, currencyString);

            toolbar.SetOnMenuItemClickListener(listener);

            //Finds the text view
            buyTextView = view.FindViewById<TextView>(Resource.Id.buyPrice);
            sellTextView = view.FindViewById<TextView>(Resource.Id.sellPrice);
            lastTextView = view.FindViewById<TextView>(Resource.Id.lastPrice);

            //Get the orders for the market
            orderBook = APIMethods.GetOrderBook(currencyString, Order.Type.both);

            //Finds the listview for buy orders
            buyOrderListView = (ListView)view.FindViewById(Resource.Id.buyOrders_listView);

            //Create the new adapter
            buyAdapter = new CurrencyOrderBookAdapter(Activity, orderBook.Buys.ToList(), true);
            buyOrderListView.Adapter = buyAdapter;

            //Set the buy orders to the last item 
            buyOrderListView.SetSelection(buyOrderListView.Adapter.Count - 1);

            //Finds the listview for sell orders
            sellOrderListView = (ListView)view.FindViewById(Resource.Id.sellOrders_listView);

            //Create the new adapter
            sellAdapter = new CurrencyOrderBookAdapter(Activity, orderBook.Sells.ToList(), false);
            sellOrderListView.Adapter = sellAdapter;

            //Set the on item click event for the listviews
            sellOrderListView.ItemClick += OrderListView_ItemClick;
            buyOrderListView.ItemClick += OrderListView_ItemClick;

            //Set the trading pair text
            var tradingPairText = (TextView)view.FindViewById(Resource.Id.tradingPairText);
            tradingPairText.Text = currencyString;

            var currencySelectedText = (TextView)view.FindViewById(Resource.Id.selectedCurrencyBalanceAvailableText);
            currencySelectedText.Text = currencyString;

            //Set the amount of available BTC to the user
            btcBalance = (TextView)view.FindViewById(Resource.Id.btcBalance);

            string btcBalanceAmount = "0.000000000";

            try
            {
                Balance b = APIMethods.GetBalance("BTC");
                btcBalanceAmount = b.balance.ToString("0.#########");
            }
            catch
            {
                Toast.MakeText(Activity, "Unable to get BTC Balance, ensure API keys are correct", ToastLength.Short).Show();
            }

            btcBalance.Text = btcBalanceAmount;

            //Set the amount of available currency for the currency selected
            selectedCurrencyBalance = (TextView)view.FindViewById(Resource.Id.selectedCurrencyBalance);

            string selectedCurrencyBalanceAmount = "0.000000000";

            try
            {
                Balance b = APIMethods.GetBalance(currencyString);
                selectedCurrencyBalanceAmount = b.balance.ToString("0.#########");
            }
            catch
            {
                Toast.MakeText(Activity, "Unable to get " + currencyString + " Balance, ensure API keys are correct", ToastLength.Short).Show();
            }

            selectedCurrencyBalance.Text = selectedCurrencyBalanceAmount;

            //Get 24 hour API data for currency
            try
            {
                 tick = APIMethods.GetTicker(currencyString);
            }
            catch (Exception e)
            {
                Toast.MakeText(Activity, e.Message.ToString(), ToastLength.Short).Show();
            }
            
            //Sets the buy and sell prices
            buyTextView.Text = tick.Bid.ToString("0.#########");
            sellTextView.Text = tick.Ask.ToString("0.#########");
            lastTextView.Text = tick.Last.ToString("0.#########");

            //Subscribe to text changed events for order section
            totalPriceBtc = (TextView)view.FindViewById(Resource.Id.totalBtcPrice);
            orderAmount = (EditText)view.FindViewById(Resource.Id.amountToPurchase);
            orderPrice = (EditText)view.FindViewById(Resource.Id.priceToPurchase);

            orderAmount.TextChanged += OrderData_TextChanged;
            orderPrice.TextChanged += OrderData_TextChanged;

            //Testing awaiting method
            var t = Task.Run(async () => {
                await RefreshOrderBook();
            });

            //invoke loop method but DO NOT await it
            //RefreshOrderBook();

            return view;
        }

        private void OrderListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var order = e.Parent.GetItemAtPosition(e.Position).ToString();

            string[] orderData = order.Split(',');

            string amount = orderData[0];
            string price = orderData[1];

            orderAmount.Text = amount;
            orderPrice.Text = price;

        }

        private void OrderData_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            double amount = Convert.ToDouble(orderAmount.Text);
            double price = Convert.ToDouble(orderPrice.Text);

            totalPriceBtc.Text = (amount * price).ToString("0.#########");
        }
        

        private async Task RefreshOrderBook()
        {
            while (MainActivity.isOnCurrencyFragment == true)
            {
                try
                {
                    //Get new orderbook
                    orderBook = APIMethods.GetOrderBook(currencyString, Order.Type.both);

                    sellAdapter.Update(orderBook.Sells.ToList(), Activity);
                    buyAdapter.Update(orderBook.Buys.ToList(), Activity);
                }
                catch
                {
                    Toast.MakeText(Activity, "Unable to update orders", ToastLength.Short).Show();
                }

                //Wait for 1 second
                await Task.Delay(1000);
            }
        }

        public static void OnMenuItemClick(Activity activity)
        {
            try
            {
                tick = APIMethods.GetTicker(Currency);

                //Sets the buy and sell prices
                buyTextView.Text = tick.Bid.ToString("N8");
                sellTextView.Text = tick.Ask.ToString("N8");
                lastTextView.Text = tick.Last.ToString("N8");

                Toast.MakeText(activity, "Refreshed", ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Toast.MakeText(activity, e.Message.ToString(), ToastLength.Short).Show();
            }
        }

    }
}