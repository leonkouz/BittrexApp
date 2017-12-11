using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace Bittrex
{
    [Activity(Label = "DetailsActivity")]
    public class DetailsActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var index = Intent.Extras.GetInt("current_currency_selected", 0);

            var details = DetailsFragment.NewInstance(index); // DetailsFragment.NewInstance is a factory method to create a Details Fragment
            var fragmentTransaction = FragmentManager.BeginTransaction();
            fragmentTransaction.Add(Android.Resource.Id.Content, details);
            fragmentTransaction.Commit();
        }
    }

    internal class DetailsFragment : Fragment
    {
        public static DetailsFragment NewInstance(int playId)
        {
            var detailsFrag = new DetailsFragment { Arguments = new Bundle() };
            detailsFrag.Arguments.PutInt("current_currency_selected", playId);
            return detailsFrag;
        }
        public int ShownCurrency
        {
            get { return Arguments.GetInt("current_currency_selected", 0); }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }
            var scroller = new ScrollView(Activity);
            var text = new TextView(Activity);
            var padding = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Activity.Resources.DisplayMetrics));
            text.SetPadding(padding, padding, padding, padding);
            text.TextSize = 24;
            scroller.AddView(text);
            return scroller;
        }
    }


}