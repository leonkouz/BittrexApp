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

        public static Activity activity;

        public static TextView buyTextView;
        public static TextView sellTextView;
        public static TextView lastTextView;
        public static Ticker tick;

        public static TextView btcBalance;
        public static TextView selectedCurrencyBalance;

        public static EditText orderAmount;
        public static EditText orderPrice;
        public static TextView totalPriceBtc;

        public static TextView currencySelectedText;

        OrderBook orderBook;
        List<OpenOrder> usersOrders;
        
        ListView buyOrderListView;
        ListView sellOrderListView;
        ListView usersOrderListView;
        CurrencyOrderBookAdapter buyAdapter;
        CurrencyOrderBookAdapter sellAdapter;
        public static YourOrdersListViewAdapter usersOrderAdapter;

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

            var topToolbar = viewMain.FindViewById<Toolbar>(Resource.Id.toolbar);

            //Hooks into the refresh button listerner
            ToolbarOnClickListener listener = new ToolbarOnClickListener(Activity, buyTextView, sellTextView, lastTextView, currencyString);

            topToolbar.SetOnMenuItemClickListener(listener);

            //Gets the bottom toolbar
            var bottomToolbar = (Toolbar)view.FindViewById(Resource.Id.currencyToolbar);
            bottomToolbar.InflateMenu(Resource.Menu.bot_currency_menu);
            bottomToolbar.MenuItemClick += BottomToolbar_MenuItemClick;

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

            //Used to handle OnClick events on users Orders cancel button
            activity = Activity;

            //Get the current open orders for the specified currency
            try
            {
                usersOrders = APIMethods.GetOpenOrders(currencyString);
            }
            catch (Exception exc)
            {
                usersOrders = new List<OpenOrder>();

                Toast.MakeText(Activity, exc.Message.ToString(), ToastLength.Short).Show();
            }

            //Find the listview for users orders
            usersOrderListView = (ListView)view.FindViewById(Resource.Id.yourOrder_listView);

            //Create a new adapter for users order listview
            usersOrderAdapter = new YourOrdersListViewAdapter(this.Context, usersOrders);
            usersOrderListView.Adapter = usersOrderAdapter;

            //Set the on item click event for the listviews
            sellOrderListView.ItemClick += OrderListView_ItemClick;
            buyOrderListView.ItemClick += OrderListView_ItemClick;

            //Set the trading pair text
            var tradingPairText = (TextView)view.FindViewById(Resource.Id.tradingPairText);
            tradingPairText.Text = currencyString;

            currencySelectedText = (TextView)view.FindViewById(Resource.Id.selectedCurrencyBalanceAvailableText);
            currencySelectedText.Text = Currency.Substring(4, Currency.Length - 4);

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
                Balance b = APIMethods.GetBalance(Currency.Substring(4, Currency.Length - 4));
                selectedCurrencyBalanceAmount = b.Available.ToString("0.#########");
            }
            catch
            {
                Toast.MakeText(Activity, "Unable to get " + currencyString + " Balance, ensure API keys are correct", ToastLength.Short).Show();
            }

            selectedCurrencyBalance.Text = selectedCurrencyBalanceAmount;

            //Subscribe to text changed events for order section
            totalPriceBtc = (TextView)view.FindViewById(Resource.Id.totalBtcPrice);
            orderAmount = (EditText)view.FindViewById(Resource.Id.amountToPurchase);
            orderPrice = (EditText)view.FindViewById(Resource.Id.priceToPurchase);

            orderAmount.TextChanged += OrderData_TextChanged;
            orderPrice.TextChanged += OrderData_TextChanged;

            //Subscribe the order buttons to on click event
            var buyButton = (Button)view.FindViewById(Resource.Id.buyOrderbutton);
            var sellButton = (Button)view.FindViewById(Resource.Id.sellOrderbutton);

            buyButton.Click += BuyButton_Click;
            sellButton.Click += SellButton_Click;

            //Testing awaiting method
            var rereshOrderBook = Task.Run(async () => {
                await RefreshOrderBook();
            });

            //Testing awaiting method
            var refreshUsersOrders = Task.Run(async () => {
                await RefreshUserOrders();
            });

            //Testing awaiting method
            var refreshAvailableBalances = Task.Run(async () => {
                await RefreshAvailableBalances();
            });

            //invoke loop method but DO NOT await it
            //RefreshOrderBook();
            return view;
        }

        private void BottomToolbar_MenuItemClick(object sender, Toolbar.MenuItemClickEventArgs e)
        {
            if (e.Item.ItemId == Resource.Id.currencyToolbar_ordersButton)
            {
                return;
            }

            if (e.Item.ItemId == Resource.Id.currencyToolbar_chartButton)
            {
                // Execute a transaction, replacing any existing fragment with this one inside the frame.
                var fragmentTransaction = FragmentManager.BeginTransaction();

                fragmentTransaction.Hide(this);

                //check if fragment has been added
                Fragment chartFragment = FragmentManager.FindFragmentByTag("chartFragment");

                if(chartFragment != null)
                {
                    if (chartFragment.IsAdded)
                    {
                        fragmentTransaction.Show(MainActivity.chartFragment);
                    }
                    else
                    {
                        fragmentTransaction.Add(Resource.Id.fragmentContainer, MainActivity.chartFragment, "chartFragment");
                    }
                }
                else
                {
                    fragmentTransaction.Add(Resource.Id.fragmentContainer, MainActivity.chartFragment, "chartFragment");
                }
                
                //fragmentTransaction.AddToBackStack(null);
                fragmentTransaction.SetTransition(FragmentTransit.FragmentFade);
                fragmentTransaction.Commit();
            }

            if(e.Item.ItemId == Resource.Id.currencyToolbar_historyButton)
            {
                return;
            }

        }

        private void SellButton_Click(object sender, EventArgs e)
        {
            //Inflate the dialog layout that should appear when a button is pressed
            LayoutInflater layoutInfalter = LayoutInflater.From(Activity);
            View mView = layoutInfalter.Inflate(Resource.Layout.OrderConfirmationLayout, null);

            //Gets a string of the currency to use as a label
            string currencyOnly = Currency.Substring(4, Currency.Length - 4);

            //Set the information of the order
            var amountTextView = (TextView)mView.FindViewById(Resource.Id.confirmAmount);
            amountTextView.Text = orderAmount.Text + " " + currencyOnly;

            var priceTextView = (TextView)mView.FindViewById(Resource.Id.confirmPrice);
            priceTextView.Text = orderPrice.Text + " BTC";

            var totalTextView = (TextView)mView.FindViewById(Resource.Id.confirmTotal);
            totalTextView.Text = totalPriceBtc.Text + " BTC";

            AlertDialog.Builder ad = new AlertDialog.Builder(Activity);
            ad.SetTitle("Confirm your sell order");
            ad.SetCancelable(false).SetPositiveButton("Confirm", delegate
            {

                double amount = Convert.ToDouble(orderAmount.Text);
                double price = Convert.ToDouble(orderPrice.Text);

                try
                {
                    string orderUUID = APIMethods.PlaceSellLimitOrder(Currency, amount, price);
                }
                catch (Exception i)
                {
                    Toast.MakeText(Activity, i.Message.ToString(), ToastLength.Short).Show();
                }

            }).SetNegativeButton("Cancel", delegate
            {
                ad.Dispose();
            });

            ad.SetView(mView);
            ad.Show();
        }

        private void BuyButton_Click(object sender, EventArgs e)
        {
            //Inflate the dialog layout that should appear when a button is pressed
            LayoutInflater layoutInfalter = LayoutInflater.From(Activity);
            View mView = layoutInfalter.Inflate(Resource.Layout.OrderConfirmationLayout, null);

            //Gets a string of the currency to use as a label
            string currencyOnly = Currency.Substring(4, Currency.Length - 4);

            //Set the information of the order
            var amountTextView = (TextView)mView.FindViewById(Resource.Id.confirmAmount);
            amountTextView.Text = orderAmount.Text + " " + currencyOnly;

            var priceTextView = (TextView)mView.FindViewById(Resource.Id.confirmPrice);
            priceTextView.Text = orderPrice.Text + " BTC";

            var totalTextView = (TextView)mView.FindViewById(Resource.Id.confirmTotal);
            totalTextView.Text = totalPriceBtc.Text + " BTC";

            AlertDialog.Builder ad = new AlertDialog.Builder(Activity);
            ad.SetTitle("Confirm your buy order");
            ad.SetCancelable(false).SetPositiveButton("Confirm", delegate
            {

                double amount = Convert.ToDouble(orderAmount.Text);
                double price = Convert.ToDouble(orderPrice.Text);

                try
                {
                    string orderUUID = APIMethods.PlaceBuyLimitOrder(Currency, amount, price);
                }
                catch (Exception i)
                {
                    Toast.MakeText(Activity, i.Message.ToString(), ToastLength.Short).Show();
                }

            }).SetNegativeButton("Cancel", delegate
            {
                ad.Dispose();
            });

            ad.SetView(mView);
            ad.Show();
        }

        private void OrderListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {

            var order = e.Parent.GetItemAtPosition(e.Position).ToString();

            if (e.Parent.Id == Resource.Id.sellOrders_listView)
            {
                double availableAmount;

                try
                {
                    availableAmount = Convert.ToDouble(selectedCurrencyBalance.Text);
                }
                catch
                {
                    availableAmount = 0;
                }

                string[] orderData = order.Split(',');

                double amount;
                try
                {
                    amount = Convert.ToDouble(orderData[0]);
                }
                catch
                {
                    amount = 0;
                }

                string price = orderData[1];

                if (availableAmount > amount)
                {
                    orderAmount.Text = amount.ToString("0.#########");
                }
                else
                {
                    orderAmount.Text = availableAmount.ToString("0.#########");
                }

                orderPrice.Text = price;

            }
            else if (e.Parent.Id == Resource.Id.buyOrders_listView)
            {
                double availableAmount;
                try
                {
                    availableAmount = Convert.ToDouble(btcBalance.Text);
                }
                catch
                {
                    availableAmount = 0;
                }

                string[] orderData = order.Split(',');

                double price; 

                try
                {
                    price = Convert.ToDouble(orderData[1]);
                }
                catch
                {
                    price = 0;
                }

                string possiblePurchaseAmount = (availableAmount / price).ToString("0.#########");
                
                orderPrice.Text = price.ToString("0.#########");
                orderAmount.Text = possiblePurchaseAmount;
            }
        }

        private void OrderData_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            double amount;
            double price;

            try
            {
                amount = Convert.ToDouble(orderAmount.Text);
            }
            catch
            {
                amount = 0;
            };

            try
            {
                price = Convert.ToDouble(orderPrice.Text);
            }
            catch
            {
                price = 0;
            }

            totalPriceBtc.Text = (amount * price).ToString("0.#########");
        }

        private async Task RefreshUserOrders()
        {
            while (MainActivity.isOnCurrencyFragment == true)
            {
                try
                {
                    //Refresh users orders
                    usersOrders = APIMethods.GetOpenOrders(currencyString);
                    usersOrderAdapter.Update(usersOrders, Activity);
                }
                catch (Exception except)
                {
                    Toast.MakeText(Activity, "Unable to update users orders: " + except.Message.ToString(), ToastLength.Short).Show();
                }

                //Wait for 1 second
                await Task.Delay(1000);
            }
        }


        private async Task RefreshAvailableBalances()
        {
            while (MainActivity.isOnCurrencyFragment == true)
            {
                //Update the selected currency balance
                string selectedCurrencyBalanceAmount = "0.000000000";
                try
                {
                    Balance b = APIMethods.GetBalance(Currency.Substring(4, Currency.Length - 4));
                    selectedCurrencyBalanceAmount = b.Available.ToString("0.#########");
                }
                catch
                {
                    Toast.MakeText(Activity, "Unable to get " + currencyString + " Balance, ensure API keys are correct", ToastLength.Short).Show();
                }

                activity.RunOnUiThread(() =>
                {
                    //Change the text on the UI thread
                    selectedCurrencyBalance.Text = selectedCurrencyBalanceAmount;
                });

                //Update the BTC currency balance
                string btcCurrencyBalanceAmount = "0.000000000";
                try
                {
                    Balance b = APIMethods.GetBalance("BTC");
                    btcCurrencyBalanceAmount = b.Available.ToString("0.#########");
                }
                catch
                {
                    Toast.MakeText(Activity, "Unable to get BTC Balance, ensure API keys are correct", ToastLength.Short).Show();
                }

                activity.RunOnUiThread(() =>
                {
                    //Change the text on the UI thread
                    btcBalance.Text = btcCurrencyBalanceAmount;
                });

                await Task.Delay(1000);
            }
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
                    Toast.MakeText(Activity, "Unable to update market orders", ToastLength.Short).Show();
                    continue;
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