using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Security.Cryptography.Encryption;

using Android.App;
using Android.Preferences;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using System.Threading.Tasks;
using System.Threading;

namespace Bittrex
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {

        private TextView apiKeyText;
        private TextView secretKeyText;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Settings);

            var apiKeyButton = (Button)FindViewById(Resource.Id.settings_apiKeyButton);
            var secretKeyButton = (Button)FindViewById(Resource.Id.settings_secretKeyButton);

            apiKeyButton.Click += ApiKeyButton_Click;
            secretKeyButton.Click += SecretKeyButton_Click;

            apiKeyText = (TextView)FindViewById(Resource.Id.settings_apiKeyTextView);
            secretKeyText = (TextView)FindViewById(Resource.Id.settings_secretKeyTextView);

            apiKeyText.Text = LoginData.APIKey;
            secretKeyText.Text = LoginData.SecretKey;
        }

        private void SecretKeyButton_Click(object sender, EventArgs e)
        {
            LayoutInflater layoutInfalter = LayoutInflater.From(this);
            View mView = layoutInfalter.Inflate(Resource.Layout.Settings_inputlayout, null);

            var inputText = (TextView)mView.FindViewById(Resource.Id.settings_inputText);

            AlertDialog.Builder ad = new AlertDialog.Builder(this);
            ad.SetTitle("Enter your new Secret Key");
            ad.SetCancelable(false).SetPositiveButton("Okay", delegate
            {
                string userInput = inputText.Text;

                //Do stuff when okay button is clicked
                secretKeyText.Text = userInput;
                LoginData.SecretKey = userInput;

                //Encrypt key
                string secretKeyEncrypted = Encrypt(userInput, LoginData.SecretPasswordString);

                //Saves the key to the local device
                var localKeys = Application.Context.GetSharedPreferences("Keys", FileCreationMode.Private);
                var keyEdit = localKeys.Edit();
                keyEdit.Remove("Secret");
                keyEdit.PutString("Secret", secretKeyEncrypted);
                keyEdit.Commit();

            }).SetNegativeButton("Cancel", delegate
            {
                ad.Dispose();
            });

            ad.SetView(mView);
            ad.Show();


        }

        private void ApiKeyButton_Click(object sender, EventArgs e)
        {
            LayoutInflater layoutInfalter = LayoutInflater.From(this);
            View mView = layoutInfalter.Inflate(Resource.Layout.Settings_inputlayout, null);

            var inputText = (TextView)mView.FindViewById(Resource.Id.settings_inputText);

            AlertDialog.Builder ad = new AlertDialog.Builder(this);
            ad.SetTitle("Enter your new API Key");
            ad.SetCancelable(false).SetPositiveButton("Okay", delegate
            {
                string userInput = inputText.Text;

                //Do stuff when okay button is clicked
                apiKeyText.Text = userInput;
                LoginData.APIKey = userInput;

                //Encrypt key
                string apiKeyEncrypted = Encrypt(userInput, LoginData.SecretPasswordString);

                //Saves the key to the local device
                var localKeys = Application.Context.GetSharedPreferences("Keys", FileCreationMode.Private);
                var keyEdit = localKeys.Edit();
                keyEdit.Remove("API");
                keyEdit.PutString("API", apiKeyEncrypted);
                keyEdit.Commit();


            }).SetNegativeButton("Cancel", delegate
            {
                ad.Dispose();
            });

            ad.SetView(mView);
            ad.Show();
        }

    }
}