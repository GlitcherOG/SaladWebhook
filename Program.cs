﻿using CefSharp;
using CefSharp.OffScreen;
using Discord;
using Discord.Webhook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WindowsFormsApp1
{
    static class Program
    {
        //Web Adresses
        //https://github.com/ravibpatel/AutoUpdater.NET
        //https://app-api.salad.io/api/v1/profile/xp
        //https://app-api.salad.io/api/v2/reports/30-day-earning-history
        //https://app-api.salad.io/api/v1/profile/
        //https://app-api.salad.io/api/v1/account/
        //https://app-api.salad.io/api/v1/reward-vault
        //https://app-api.salad.io/api/v1/notification-banner
        //https://app-api.salad.io/api/v1/profile/referral-code
        //https://app-api.salad.io/api/v1/profile/selected-reward
        //https://app-api.salad.io/api/v1/profile/referrals
        //https://app-api.salad.io/api/v2/storefront
        //https://app-api.salad.io/api/v1/rewards/
        //https://app-api.salad.io/login
        //https://app-api.salad.io/logout
        //https://app-api.salad.io/api/v2/changelog
        //https://app-api.salad.io/api/v2/seasons/current
        //https://app-api.salad.io/api/v2/bonuses/earning-rate
        //https://app-api.salad.io/api/v2/avatars
        //https://app-api.salad.io/api/v2/avatars/selected
        //   new Level('carrot', 'Carrot', 0, 20),
        //   new Level('lettuce', 'Lettuce', 20, 50),
        //   new Level('tomato', 'Tomato', 50, 100),
        //   new Level('cucumber', 'Cucumber', 100, 1000),
        //   new Level('beet', 'Beet', 1000, 5000),
        //   new Level('spinach', 'Spinach', 5000, 10000),
        //   new Level('mushroom', 'Mushroom', 10000, 20000),
        //   new Level('red-pepper', 'Red Pepper', 20000, 30000),
        //   new Level('avocado', 'Avocado', 30000, 50000),
        //   new Level('red-onion', 'Red Onion', 50000, 75000),
        //   new Level('olives', 'Olives', 75000, 100000),
        //   new Level('broccoli', 'Broccoli', 100000, 150000),
        //   new Level('blue-cheese', 'Blue Cheese', 150000, 250000)

        static CefSharp.OffScreen.ChromiumWebBrowser chromiumWebBrowser1;
        //static CefSharp.OffScreen.ChromiumWebBrowser chromiumWebBrowser2;
        static NoficationIcon Icon;
        public static List<GameData> ProductTracking = new List<GameData>();
        public static List<bool> WishlistCheck = new List<bool>();
        public static List<GameData> StoreProducts = new List<GameData>();
        public static List<int> FruitXp = new List<int>();
        public static List<string> AmmountCheck = new List<string>();
        public static SettingsSaveLoad Saving = new SettingsSaveLoad();
        public static ProductDataSaveLoad ProductDataSaving = new ProductDataSaveLoad();
        static List<JsonDetails> Temp = new List<JsonDetails>();
        public static int waittime = 15;
        public static bool postIfChange = true;
        public static bool postIfStoreChange = false;
        public static string Webhook = "";
        public static string username = "";
        static string Balance = "0.000";
        static string OldBalance = "0";
        static string lifetimeBalance;
        static string lifetimeXP;
        static string ReferalCode;
        static bool Wait;

        [STAThread]
        static void Main()
        {
            //GenerateFruit();
            AutoUpdater.ShowSkipButton = false;
            //AutoUpdater.Start("https://raw.githubusercontent.com/GlitcherOG/Salad-Webhook/main/Update.xml");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var settings = new CefSettings();
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaladWebHook\\CefSharp\\Cache");
            Cef.Initialize(settings);
            LoadSavedData();
            Icon = new NoficationIcon();
            Application.Run(Icon);
        }
        ////public static void GenerateFruit()
        //{
        //    FruitXp.Add(60);
        //    FruitXp.Add(180);
        //    FruitXp.Add(420);
        //    FruitXp.Add(780);
        //    FruitXp.Add(1380);
        //    FruitXp.Add(2280);
        //    FruitXp.Add(3660);
        //    FruitXp.Add(5700);
        //    FruitXp.Add(8580);
        //    FruitXp.Add(12540);
        //    FruitXp.Add(17940);
        //    FruitXp.Add(25080);
        //    FruitXp.Add(34440);
        //    FruitXp.Add(46380);
        //    FruitXp.Add(61440);

        //    //FruitXp.Add(34440);
        //    //FruitXp.Add(46380);
        //    //FruitXp.Add(61440);
        //}
        public static void LoadSavedData()
        {
            Saving = SettingsSaveLoad.Load();
            ProductDataSaving = ProductDataSaveLoad.Load();
            if (Saving != null)
            {
                waittime = (int)Saving.waitTimeMin;
                postIfChange = Saving.onlyIfNewPost;
                Webhook = Saving.webhook;
                postIfStoreChange = Saving.postIfStoreChange;
            }
            else
            {
                Saving = new SettingsSaveLoad();
                LoginLogout loginform = new LoginLogout();
                loginform.Show();
                Settings Wsettings = new Settings();
                Wsettings.Show();
            }
            if (ProductDataSaving != null)
            {
                ProductTracking = ProductDataSaving.TrackedProducts;
                for (int i = 0; i < ProductTracking.Count; i++)
                {
                    WishlistCheck.Add(false);
                    AmmountCheck.Add(null);
                }
                StoreProducts = ProductDataSaving.AllProducts;
            }
            else
            {
                ProductDataSaving = new ProductDataSaveLoad();
            }
        }
        public static void LoadEarnings()
        {
            chromiumWebBrowser1 = new ChromiumWebBrowser();
            Task.Run(() => Startup());
        }

        private static async Task Startup()
        {
            await CheckPrices();
            //while(Wait)
            //{
            //    await Task.Delay(100);
            //}
            //await Refresh();
            //await ProfileData();
            //await RefreshTimer();

        }

        private static async Task RefreshTimer()
        {
            while (true)
            {
                Debug.WriteLine("Here");
                await Task.Delay(1000 * waittime * 60);
                while(Wait)
                {
                    Debug.WriteLine("Waiting");
                    await Task.Delay(100);
                }
                if (username == null || username == "")
                {
                    Debug.WriteLine("Loading profiledata");
                    await Task.Run(() => ProfileData());
                }
                else
                {
                    //await Refresh();
                    Debug.WriteLine("Checking Data");
                    await CheckData();
                }
                await Task.Delay(5000);
                Debug.WriteLine("Checking Prices");
                await CheckPrices();
                //await CheckStore();
                //chromiumWebBrowser1 = new ChromiumWebBrowser();
                //chromiumWebBrowser2 = new ChromiumWebBrowser();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static async Task AddProductA(string Address)
        {
            bool Test = false;
            for (int i = 0; i < ProductTracking.Count; i++)
            {
                if (Address.Substring(40) == ProductTracking[i].id)
                {
                    Test = true;
                    ProductTracking.RemoveAt(i);
                    NoficationIcon.storeform.UpdateButton();
                    ProductDataSaving.Save();
                    WishlistCheck.RemoveAt(i);
                    AmmountCheck.RemoveAt(i);
                }
            }
            if (!Test)
            {
                string temp2 = await LoadWebPage(Address);
                ProductTracking.Add(LoadGameData(temp2));
                WishlistCheck.Add(false);
                AmmountCheck.Add(null);
                //GameData temp = ProductTracking[0];
                //temp.price = "100";
                //ProductTracking[0] = temp;
                NoficationIcon.storeform.UpdateButton();
                ProductDataSaving.Save();
            }
        }

        public static void AddProduct(string Address)
        {
            Task.Run(() => AddProductA(Address));
        }

        private static async Task CheckStore()
        {
            if (postIfStoreChange)
            {
                string uri = "https://app-api.salad.io/api/v1/rewards/";
                string temp3 = LoadWebPage(uri, true).Result;
                temp3 = temp3.Replace("[{", "");
                temp3 = temp3.Replace("}]", "}");
                string[] temp2 = temp3.Split(new string[] { ",{" }, StringSplitOptions.None);
                List<GameData> Store = new List<GameData>();
                for (int i = 0; i < temp2.Length; i++)
                {
                    Store.Add(LoadGameData("{" + temp2[i]));
                }
                string StorePriceChanges = "";
                string StoreProductChanges = "";
                for (int i = 0; i < Store.Count; i++)
                {
                    bool test = false;
                    for (int a = 0; a < StoreProducts.Count; a++)
                    {
                        if (Store[i].id == StoreProducts[a].id)
                        {
                            test = true;
                            if (Store[i].price != StoreProducts[a].price)
                            {
                                float temp = float.Parse(Store[i].price) - float.Parse(StoreProducts[a].price);
                                string tempbal;
                                if (temp > 0)
                                {
                                    tempbal = " ($+" + Math.Round(temp, 4).ToString() + ")";
                                }
                                else
                                {
                                    tempbal = " ($" + Math.Round(temp, 4).ToString() + ")";
                                }
                                StorePriceChanges += Store[i].name + ": $" + Store[i].price + " ($" + Store[i].price + ")" + Environment.NewLine;
                            }
                            break;
                        }
                    }
                    if (!test)
                    {
                        StoreProductChanges += Store[i].name + ": $" + Store[i].price + "" + Environment.NewLine;
                    }
                }
                var client = new DiscordWebhookClient(Webhook);

                var embed = new EmbedBuilder
                {
                    Title = "Store Changes",
                };
                if (StorePriceChanges != "" || StoreProductChanges != "")
                {
                    if (StorePriceChanges != "" && StorePriceChanges.Length <= 2048)
                    {
                        if (StorePriceChanges.Length <= 2048)
                        {
                            embed.AddField("Price Changes", StorePriceChanges);
                        }
                        else
                        {
                            embed.AddField("Price Changes", Regex.Matches(StorePriceChanges, "\n").Count.ToString() + " Items have changed prices");
                        }
                    }
                    //if (StoreProductChanges != "")
                    //{
                    //    if (StoreProductChanges.Length <= 2048)
                    //    {
                    //        embed.AddField("Product Changes", StoreProductChanges);
                    //    }
                    //    else
                    //    {
                    //        embed.AddField("Product Changes", Regex.Matches(StoreProductChanges, "\n").Count.ToString() + " Items have been added");
                    //    }
                    //}
                    embed.Timestamp = DateTimeOffset.Now;
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO Shop", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                    StoreProducts = Store;
                    ProductDataSaving.Save();
                }
            }
        }

        private static async Task CheckPrices()
        {
            Wait = true;
            var client = new DiscordWebhookClient(Webhook);
            string PriceChange = "";
            string QuantityChange = "";
            string EarntEnough = "";
            if (ProductTracking.Count != 0)
            {
                for (int i = 0; i < ProductTracking.Count; i++)
                {
                    Debug.WriteLine(i);
                    string temp2 = await LoadWebPage("https://app-api.salad.io/api/v1/rewards/" + ProductTracking[i].id);
                    if (temp2 != "")
                    {
                        GameData data = LoadGameData(temp2);
                        if (ProductTracking[i].price != data.price)
                        {
                            float temp = float.Parse(ProductTracking[i].price) - float.Parse(data.price);
                            string tempbal;
                            if (temp > 0)
                            {
                                tempbal = " ($+" + Math.Round(temp, 4).ToString() + ")";
                            }
                            else
                            {
                                tempbal = " ($" + Math.Round(temp, 4).ToString() + ")";
                            }
                            PriceChange += data.name + ": $" + Math.Round(float.Parse(data.price), 2).ToString() + " " + tempbal + Environment.NewLine;
                        }
                        if (ProductTracking[i].quantity != data.quantity)
                        {
                            if (data.quantity != "" && data.quantity != null)
                            {
                                if (float.Parse(data.quantity) <= 5)
                                {
                                    QuantityChange += data.name + " (" + data.quantity + " remaning)" + Environment.NewLine;
                                }
                                else if (float.Parse(data.quantity) == 0)
                                {
                                    QuantityChange += data.name + "(Out of Stock)" + Environment.NewLine;
                                }
                                else if (float.Parse(data.quantity) > float.Parse(ProductTracking[i].quantity))
                                {
                                    QuantityChange += data.name + " (" + data.quantity + " Now in Stock)" + Environment.NewLine;
                                }
                            }
                        }
                        if (!WishlistCheck[i])
                        {
                            float temp = float.Parse(ProductTracking[i].price);
                            float temp3 = float.Parse(Balance);
                            if (temp <= temp3)
                            {
                                EarntEnough += data.name + ": $" + Math.Round(float.Parse(data.price), 2).ToString() + Environment.NewLine;
                                ProductTracking[i] = data;
                                WishlistCheck[i] = true;
                            }
                        }
                        ProductTracking[i] = data;
                    }
                    else
                    {
                        GameData data = ProductTracking[i];
                        data.quantity = "-1";
                        ProductTracking[i] = data;
                    }
                }
                ProductDataSaving.Save();
                var embed = new EmbedBuilder
                {
                    Title = "Congradulations You Have Chopped Enought For:",
                };
                embed.Color = Color.Green;
                embed.Timestamp = DateTimeOffset.Now;
                embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png";
                if (EarntEnough != "")
                {
                    if (EarntEnough.Length <= 2048)
                    {
                        embed.Description = EarntEnough;
                    }
                    else
                    {
                        embed.Description = Regex.Matches(EarntEnough, "\n").Count.ToString() + " Items";
                    }
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                }
                if (QuantityChange != "")
                {
                    embed.Title = "Wishlist Quanity Change:";
                    if (QuantityChange.Length <= 2048)
                    {
                        embed.Description = QuantityChange;
                    }
                    else
                    {
                        embed.Description = Regex.Matches(QuantityChange, "\n").Count.ToString() + " Items have changed the ammount remaining.";
                    }
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                }
                if (PriceChange != "")
                {
                    embed.Title = "Price Change:";
                    embed.Description = PriceChange;
                    if (PriceChange.Length <= 2048)
                    {
                        embed.Description = PriceChange;
                    }
                    else
                    {
                        embed.Description = Regex.Matches(PriceChange, "\n").Count.ToString() + " Items have changed prices.";
                    }
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                }
            }
            Wait = false;
        }

        private static async Task Refresh()
        {
            string uri = "https://app.salad.io/earn/summary";
            chromiumWebBrowser1.Load(uri);
            await Task.Delay(1000);
            while (chromiumWebBrowser1.IsLoading)
            {
                await Task.Delay(1000);
            }
            await Task.Delay(3000);
        }

        private static async Task<string> LoadWebPage(string uri, bool wait = false)
        {
            chromiumWebBrowser1.Load(uri);
            //if(wait)
            //{
            //    await Task.Delay(5000);
            //}
            await Task.Delay(1000);
            while (chromiumWebBrowser1.IsLoading)
            {
                await Task.Delay(1000);
            }
            string temp;
            Task<string> task = chromiumWebBrowser1.GetSourceAsync();
            while (task == null && task.Result == "")
            {
                task = Task.Run(() => chromiumWebBrowser1.GetSourceAsync());
            }
            temp = task.Result.TrimStart("<html><head></head><body><pre style =\"word-wrap: break-word; white-space: pre-wrap;\">".ToCharArray());
            temp = temp.TrimEnd("</pre></body></html>".ToCharArray());
            return temp;
        }

        public static dynamic LoadJson(string Json = null)
        {
            string temp = Json;
            dynamic temp2 = new List<JsonDetails>();
            temp = temp.TrimStart("<html><head></head><body><pre style =\"word-wrap: break-word; white-space: pre-wrap;\">".ToCharArray());
            temp = temp.TrimEnd("</pre></body></html>".ToCharArray());
            temp2 = JValue.Parse(Json);
            return temp2;
        }

        private static async Task ProfileData()
        {
            await Refresh();
            string temp = await LoadWebPage("https://app-api.salad.io/api/v1/profile/");
            dynamic temp2 = LoadJson(temp);
            username = temp2.username;
            if (username != null && username != "")
            {
                temp = await LoadWebPage("https://app-api.salad.io/api/v1/profile/referral-code");
                temp2 = LoadJson(temp);
                ReferalCode = temp2.code;
                Icon.UpdateTooltip();
                await Task.Run(() => CheckData());
                //await CheckData();
            }
            else
            {
                Icon.UpdateTooltip();
            }
        }

        private static async Task CheckData()
        {
            string temp3 = await LoadWebPage("https://app-api.salad.io/api/v1/profile/balance");
            bool test = false;
            dynamic temp2 = LoadJson(temp3);
            string temp4 = temp2.currentBalance;
            if (Balance != temp4)
            {
                test = true;
            }
            Balance = temp4;
            if (OldBalance == "0")
            {
                OldBalance = Balance;
            }
            lifetimeBalance = temp2.lifetimeBalance;
            temp3 = await LoadWebPage("https://app-api.salad.io/api/v1/profile/xp");
            temp2 = LoadJson(temp3);
            lifetimeXP = temp2.lifetimeXp;
            if (!postIfChange || test && postIfChange)
            {
                if (Webhook != null && Webhook != "")
                {
                    var client = new DiscordWebhookClient(Webhook);

                    var embed = new EmbedBuilder
                    {
                        Title = username,
                    };
                    string tempbal = "";
                    if (OldBalance != Balance)
                    {
                        float temp = float.Parse(Balance) - float.Parse(OldBalance);
                        OldBalance = Balance;
                        if (temp > 0)
                        {
                            embed.Color = Color.Green;
                            tempbal = " ($+" + Math.Round(temp, 3).ToString("#,##0.###") + ")";
                        }
                        else
                        {
                            embed.Color = Color.Red;
                            tempbal = " ($" + Math.Round(temp, 3).ToString("#,##0.###") + ")";
                            for (int i = 0; i < WishlistCheck.Count; i++)
                            {
                                WishlistCheck[i] = false;
                            }
                        }
                    }
                    embed.Description = "Current Balance: $" + Math.Round(float.Parse(Balance), 3).ToString("#,##0.###") + tempbal + Environment.NewLine + "Lifetime Earnings: $" + Math.Round(float.Parse(lifetimeBalance), 3).ToString("#,##0.###") + Environment.NewLine + "Livetime XP: " + float.Parse(lifetimeXP).ToString("#,##0.###");
                    embed.Footer = new EmbedFooterBuilder { Text = "Referal Code: " + ReferalCode };
                    embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png";
                    await client.SendMessageAsync("", false, embeds: new[] { embed.Build() }, "Salad.IO", "https://cdn.discordapp.com/attachments/814311805689528350/820600423512932382/logo.png");
                }
                else
                {
                    MessageBox.Show("Please Check Settings and Paste in a Discord Webhook", "Error");
                }
            }
        }
        //public static dynamic StripJson(string JsonFile)
        //{
        //    return JValue.Parse(JsonFile);
        //    List<JsonDetails> file = new List<JsonDetails>();
        //    string[] Dump;
        //    string Dump2;
        //    List<string> Dump3 = new List<string>();
        //    JsonDetails temp = new JsonDetails();
        //    Dump2 = JsonFile.Replace("{", "");
        //    Dump2 = Dump2.Replace("}", "");
        //    Dump = Dump2.Split(new string[] { ",\"" }, StringSplitOptions.None);
        //    string boo = "";
        //    for (int a = 0; a < Dump.Length; a++)
        //    {
        //        if (Dump[a].Contains(":"))
        //        {
        //            if (Dump[a].Contains("[") && !Dump[a].Contains("]"))
        //            {
        //                boo += Dump[a];
        //            }
        //            else
        //            {
        //                Dump3.Add(Dump[a]);
        //            }
        //        }
        //        else
        //        {
        //            boo += Dump[a];
        //            if (Dump[a].Contains("]"))
        //            {
        //                Dump3.Add(boo);
        //                boo = "";
        //            }
        //        }
        //    }
        //    for (int i = 0; i < Dump3.Count; i++)
        //    {
        //        string[] Lines = Dump3[i].Replace("\"", "").Split(':');
        //        temp.Line1 = Lines[0].ToLower();
        //        temp.Line2 = Lines[1].Replace("\n", Environment.NewLine);
        //        file.Add(temp);
        //    }
        //    return file;
        //}

        public static GameData LoadGameData(string Json)
        {
            dynamic temp = LoadJson(Json);
            GameData data = new GameData();
            data.id = temp.id;
            data.name = temp.name;
            data.price = temp.price;
            data.image = temp.coverImage;
            data.quantity = temp.quantity;
            if (temp.description != null && temp.description != "")
            {
                string temp2 = temp.description;
                data.description = temp2.Replace("<div>", "");
                data.description = data.description.Replace("<p>", "");
                data.description = data.description.Replace("<br />", "");
                data.description = data.description.Replace("</p>", "");
                data.description = data.description.Replace("</p>", "");
                data.description = data.description.Replace("<ul>", "");
                data.description = data.description.Replace("<li>", "");
                data.description = data.description.Replace("</li>", "");
                data.description = data.description.Replace("</ul>", "");
                data.description = data.description.Replace("</div>", "");
            }
            return data;
        }
    }
}
[Serializable]
public struct GameData
{
    public string id;
    public string name;
    public string price;
    public bool Checked;
    public string description;
    public string quantity;
    public string category;
    public string image;
}
[Serializable]
public struct JsonDetails
{
    public string Line1;
    public string Line2;
}
