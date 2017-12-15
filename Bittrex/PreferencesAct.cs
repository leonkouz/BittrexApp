using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Bittrex
{
    [Activity(Label = "PreferencesActivity")]
    public class PreferencesAct : Android.Preferences.PreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.preferences);

            FragmentManager.BeginTransaction().Replace(Resource.Id.fragmentContainer, new Prefs1Fragment()).Commit();

        }

        public override void OnBuildHeaders(IList<Header> target)
        {
            LoadHeadersFromResource(Resource.Layout.preferenceheaders, target);
        }

        protected override bool IsValidFragment(string fragmentName)
        {
            return true;
        }

        public class Prefs1Fragment : PreferenceFragment
        {
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);

                // Make sure default values are applied.  In a real app, you would
                // want this in a shared function that is used to retrieve the
                // SharedPreferences wherever they are needed.
                PreferenceManager.SetDefaultValues(Activity, Resource.Layout.preferences, false);

                AddPreferencesFromResource(Resource.Layout.preferenceheaders);

            }
        }
    }
}