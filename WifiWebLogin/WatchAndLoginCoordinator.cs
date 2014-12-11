using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace WifiWebLogin
{
    public class WatchAndLoginCoordinator
    {
        private readonly List<WifiLogin> _wifiLogins;
        private readonly Task _runningTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public WatchAndLoginCoordinator(List<WifiLogin> wifiLogins)
        {
            _wifiLogins = wifiLogins;
            if (wifiLogins == null) throw new ArgumentNullException("wifiLogins");
            _runningTask = WatchInBackground(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public bool IsRunning()
        {
            return _runningTask != null && !_runningTask.IsCompleted;
        }

        private async Task WatchInBackground(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var requiredLogin = await IsLoginRequired(token);
                if (null != requiredLogin)
                {
                    await PerformLogin(token);
                }
                else
                {
                    await Wait(token);
                }
            }
        }

        [Conditional("DEBUG")]
        private static void DebugWriteLine(string text)
        {
            Debug.WriteLine("{0:T}: {1}", DateTime.Now, text);
        }

        private static async Task Wait(CancellationToken token)
        {
            DebugWriteLine("Waiting...");
            await Task.Delay(TimeSpan.FromSeconds(10), token);
        }

        private async Task PerformLogin(CancellationToken token)
        {
            DebugWriteLine("Performing Login...");
            var client = new HttpClient();
            var postResult = await client.PostAsync("https://***/login.html",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("buttonClicked", "4"),
                    new KeyValuePair<string, string>("redirect_url", "www.google.de/"),
                    new KeyValuePair<string, string>("err_flag", "0"),
                    new KeyValuePair<string, string>("info_flag", "0"),
                    new KeyValuePair<string, string>("info_msg", "0"),
                    new KeyValuePair<string, string>("username", "***"),
                    new KeyValuePair<string, string>("password", "***"),
                }), token);

            postResult.EnsureSuccessStatusCode();
            var content = await postResult.Content.ReadAsStringAsync();
            DebugWriteLine(content.Contains("Login Successful") ? "Login successful" : "LOGIN FAILED");
        }

        private async Task<WifiLogin> IsLoginRequired(CancellationToken token)
        {
            try
            {
                DebugWriteLine("Checking IsLoginRequired...");

                var client = new HttpClient();
                var result = await client.GetAsync("http://www.google.de", HttpCompletionOption.ResponseHeadersRead, token);

                if (result.Headers.Location == null) return null;

                DebugWriteLine("Login is required!");

                var requiredLogin = _wifiLogins
                    .Where(l => l.DetectionType == DetectionTypeEnum.DetectByRedirect)
                    .FirstOrDefault(login => String.Equals(login.RedirectHost, result.Headers.Location.Host, StringComparison.OrdinalIgnoreCase));

                if (requiredLogin != null)
                {
                    DebugWriteLine("Login for '"+requiredLogin.ReadableName+"' required!");
                    return requiredLogin;
                }
                
                DebugWriteLine("Detected a needed login, because there was a redirection to host '"+result.Headers.Location.Host+"' but could not find any configured login!");
                return null;
            }
            catch (Exception ex)
            {
                DebugWriteLine("Error! " + ex);
                return null;
            }

        }
    }
}