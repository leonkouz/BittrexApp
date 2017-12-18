using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Preferences;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace Bittrex
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Settings);

            var apiKeyButton = (Button)FindViewById(Resource.Id.settings_apiKeyButton);
            var secretKeyButton = (Button)FindViewById(Resource.Id.settings_secretKeyButton);

            apiKeyButton.Click += ApiKeyButton_Click;
            secretKeyButton.Click += SecretKeyButton_Click;

            var apiKeyText = (TextView)FindViewById(Resource.Id.settings_apiKeyTextView);
            var secretKeyText = (TextView)FindViewById(Resource.Id.settings_secretKeyTextView);

            apiKeyText.Text = LoginData.APIKey;
            secretKeyText.Text = LoginData.SecretKey;

        }

        private void SecretKeyButton_Click(object sender, EventArgs e)
        {
            EditText et = new EditText(this);
            AlertDialog.Builder ad = new AlertDialog.Builder(this);
            ad.SetTitle("Enter your new Secret Key");
            ad.SetView(et);
            ad.Show();

        }

        private void ApiKeyButton_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder ad = new AlertDialog.Builder(this);
            ad.SetTitle("Enter your new API Key");
            ad.SetView(Resource.Layout.Settings_inputlayout);
            ad.Show();

        }
    }
}