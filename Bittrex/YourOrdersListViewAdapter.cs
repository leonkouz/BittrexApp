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
using System.Threading.Tasks;

namespace Bittrex
{
    public class YourOrdersListViewAdapter : BaseAdapter<string>
    {
        public static IList<OpenOrder> _openOrders;
        public readonly Context _context;

        public YourOrdersListViewAdapter(Context context, IList<OpenOrder> orders)
        {
            _openOrders = orders;
            _context = context;

        }

        public override string this[int position]
        {
            get
            {
                return _openOrders[position].OrderUuid;
            }
        }

        public override int Count
        {
            get { return _openOrders.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public void Update(IList<OpenOrder> orders, Activity activity)
        {
            _openOrders = orders;
            activity.RunOnUiThread(() => this.NotifyDataSetChanged());
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            var openOrder = _openOrders[position];
            var view = convertView;

            if (view == null)
            {
                var inflater = LayoutInflater.FromContext(_context);
                view = inflater.Inflate(Resource.Layout.YourOrderListViewItem, parent, false);
            }

            view.FindViewById<TextView>(Resource.Id.yourOrder_orderType).Text = openOrder.OrderType;
            view.FindViewById<TextView>(Resource.Id.yourOrder_pricePerUnit).Text = openOrder.Limit.ToString("0.#########");
            view.FindViewById<TextView>(Resource.Id.yourOrder_amountRemaining).Text = openOrder.QuantityRemaining.ToString("0.#########");
            view.FindViewById<TextView>(Resource.Id.yourOrder_amountTotal).Text = openOrder.Quantity.ToString("0.#########");
            view.FindViewById<TextView>(Resource.Id.yourOrder_totalBtcPrice).Text = (openOrder.Limit * openOrder.Quantity).ToString("0.#########");
            view.FindViewById<Button>(Resource.Id.yourOrder_cancelButton).Click += CancelOrderButton_Click;

            return view;
        }

        public async void CancelOrderButton_Click(object sender, EventArgs e)
        {
            int buttonPosition = (int)(((Button)sender).GetTag(Resource.Id.yourOrder_cancelButton));

            OpenOrder orderItem = YourOrdersListViewAdapter._openOrders[buttonPosition];

            //Testing awaiting method
            var task = Task.Run(async () => {
                await CancelOrder(orderItem);
            });
        }

        public async Task CancelOrder(OpenOrder order)
        {
            try
            {
                //Cancel Order
                APIMethods.CancelOrder(order.OrderUuid);

                //Get new order
                var newOpenOrderList = APIMethods.GetOpenOrders(order.Exchange);

                //update list
                CurrencyFragment.usersOrderAdapter.Update(newOpenOrderList, CurrencyFragment.activity);
            }
            catch
            {
                Toast.MakeText(_context, "Unable to update orders", ToastLength.Short).Show();
            }

            await Task.Delay(1);
        }
    }
}