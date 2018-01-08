using System;
using BittrexAPI;
using static System.Security.Cryptography.Encryption;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Text;
using Android;
using Android.Content.PM;
using Android.Support.Design.Widget;
using Android.Util;

namespace Bittrex
{
    [Activity(Label = "LoginActivity", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        readonly string[] PermissionsLocation =
        {
            Manifest.Permission.WriteSettings,
            Manifest.Permission.WriteSecureSettings,
            Manifest.Permission.WriteExternalStorage,
        };

        const int RequestLocationId = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //DeleteLocalDataTest();

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Login);

            const string permission = Manifest.Permission.AccessFineLocation;

            if (CheckSelfPermission(permission) == (int)Permission.Granted)
            {
                //continue
            }
            else
            {
                RequestPermissions(PermissionsLocation, RequestLocationId);
            }

            CreateSecretPassword();

            //Checks if there are any keys already stored locally
            bool keysExist = CheckLocalKeysExist();

            //If keys exist
            if (keysExist == true)
            {
                //Starts the main activity
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
                this.Finish();
            }

            //Initializing button from layout
            Button login = FindViewById<Button>(Resource.Id.login);

            //Finds the EditText boxes
            EditText apiKeyBox = FindViewById<EditText>(Resource.Id.APIKey);
            EditText secretKeyBox = FindViewById<EditText>(Resource.Id.SecretKey);

            //Login button click action
            login.Click += (object sender, EventArgs e) =>
            {

                //Get the text from the boxxes
                string apiKey = apiKeyBox.Text;
                string apiKeyEncrypted = Encrypt(apiKey, LoginData.SecretPasswordString);
                string secretKey = secretKeyBox.Text;
                string secretKeyEncrypted = Encrypt(secretKey, LoginData.SecretPasswordString);

                //Checks to see if either of the fields are empty
                if ((apiKey == "" || apiKey == null) && (secretKey == "" || secretKey == null))
                {
                    Toast.MakeText(this, "API Key and Secret Key must be entered", ToastLength.Short).Show();
                    return;
                }
                else if (apiKey == "" || apiKey == null)
                {
                    Toast.MakeText(this, "API Key must be entered", ToastLength.Short).Show();
                    return;
                }
                else if (apiKey == "" || apiKey == null)
                {
                    Toast.MakeText(this, "Secret Key must be entered", ToastLength.Short).Show();
                    return;
                }

                //Saves the keys to the local device
                var localKeys = Application.Context.GetSharedPreferences("Keys", FileCreationMode.Private);
                var keyEdit = localKeys.Edit();
                keyEdit.PutString("API", apiKeyEncrypted);
                keyEdit.PutString("Secret", secretKeyEncrypted);
                keyEdit.Commit();

                LoginData.APIKey = apiKey;
                LoginData.SecretKey = secretKey;

                //Starts the main activity
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
                this.Finish();
            };
        }



        public bool CheckLocalKeysExist()
        {
            //Retrieves data from local storage
            var localKeys = Application.Context.GetSharedPreferences("Keys", FileCreationMode.Private);
            string apiKey = Decrypt(localKeys.GetString("API", null), LoginData.SecretPasswordString);
            string secretKey = Decrypt(localKeys.GetString("Secret", null), LoginData.SecretPasswordString);

            if ((apiKey == "" || apiKey == null) && (secretKey == "" || secretKey == null))
            {
                return false;
            }
            else
            {
                //Stores the keys to be used by the application
                LoginData.APIKey = apiKey;
                LoginData.SecretKey = secretKey;
                return true;
            }
        }

        public void CreateSecretPassword()
        {
            var secretPassword = Application.Context.GetSharedPreferences("Password", FileCreationMode.Private);

            //If password is null or does not exist create a password
            if (secretPassword.GetString("Password", null) == "" || secretPassword.GetString("Password", null) == null)
            {
                string password = CreatePassword(512);

                var passwordEdit = secretPassword.Edit();
                passwordEdit.PutString("Password", password);
                passwordEdit.Commit();

                LoginData.SecretPasswordString = password;
            }

            LoginData.SecretPasswordString = secretPassword.GetString("Password", null);
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }


        public void DeleteLocalDataTest()
        {
            var localKeys = Application.Context.GetSharedPreferences("Keys", FileCreationMode.Private);
            var keyEdit = localKeys.Edit();
            keyEdit.Remove("API");
            keyEdit.Remove("Secret");
            keyEdit.Commit();
        }

    }
}