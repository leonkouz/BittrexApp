using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Object = Java.Lang.Object;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace Bittrex
{
    public class SearchableAdapter : BaseAdapter<string>, IFilterable
    {
        public static List<string> _originalData;
        public List<string> _currencies;
        private readonly Activity _context;

        public SearchableAdapter(Activity activity, List<string> currencies)
        {
            _currencies = currencies;
            _originalData = currencies;
            _context = activity;

            Filter = new CurrenciesFilter(this);
        }

        public override int Count => _currencies.Count();

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _currencies[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = _context.LayoutInflater.Inflate(Resource.Layout.list_item, null);
            view.FindViewById<TextView>(Resource.Id.Text1).Text = item;
            return view;
        }

        public Filter Filter { get; private set; }

        public override string this[int position]
        {
            get { return _currencies[position]; }
        }
    }

    public class ViewHolder
    {
        public TextView text;
    }

    class CurrenciesFilter : Filter
    {
        private readonly SearchableAdapter _adapter;

        public CurrenciesFilter(SearchableAdapter adapter)
        {
            _adapter = adapter;
        }

        protected override FilterResults PerformFiltering(ICharSequence constraint)
        {
            string filterString = constraint.ToString().ToLower();

            FilterResults results = new FilterResults();

            List<string> list = SearchableAdapter._originalData;

            int count = list.Count();
            List<string> nlist = new List<string>(count);

            string filterableString;

            for (int i = 0; i < count; i++)
            {
                filterableString = list[i];
                if (filterableString.ToLower().Contains(filterString))
                {
                    nlist.Add(filterableString);
                }
            }

            results.Values = nlist.ToJavaObject();
            results.Count = nlist.Count();

            return results;
        }

        protected override void PublishResults(ICharSequence constraint, FilterResults results)
        {
            _adapter._currencies = results.Values.ToNetObject<List<string>>();
            
           _adapter.NotifyDataSetChanged();

            // Don't do this and see GREF counts rising
            constraint.Dispose();
            results.Dispose();
        }
    }

    public class JavaHolder : Java.Lang.Object
    {
        public readonly object Instance;

        public JavaHolder(object instance)
        {
            Instance = instance;
        }
    }

    public static class ObjectExtensions
    {
        public static TObject ToNetObject<TObject>(this Java.Lang.Object value)
        {
            if (value == null)
                return default(TObject);

            if (!(value is JavaHolder))
                throw new InvalidOperationException("Unable to convert to .NET object. Only Java.Lang.Object created with .ToJavaObject() can be converted.");

            TObject returnVal;
            try { returnVal = (TObject)((JavaHolder)value).Instance; }
            finally { value.Dispose(); }
            return returnVal;
        }

        public static Java.Lang.Object ToJavaObject<TObject>(this TObject value)
        {
            if (Equals(value, default(TObject)) && !typeof(TObject).IsValueType)
                return null;

            var holder = new JavaHolder(value);

            return holder;
        }
    }
}