using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WifiWebLogin
{
    public class WatchAndLoginCoordinator
    {
        private readonly WifiLoginConfig _config;
        private readonly Task _runningTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public WatchAndLoginCoordinator(WifiLoginConfig config)
        {
            _config = config;
            if (config == null) throw new ArgumentNullException("config");
            _runningTask = WatchInBackground(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _runningTask.Wait();
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
                    await PerformLogin(requiredLogin, token);
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

        private async Task PerformLogin(WifiLogin requiredLogin, CancellationToken token)
        {
            DebugWriteLine("Performing Login...");
            var client = new HttpClient();
            //TODO: somehow we need to get to know this stuff in a generic way
            var loginUrl = "https://" + requiredLogin.RedirectHost + "/login.html";
            var postResult = await client.PostAsync(loginUrl,
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    //TODO: somehow we need to get to know this stuff in a generic way
                    new KeyValuePair<string, string>("buttonClicked", "4"),
                    new KeyValuePair<string, string>("redirect_url", "www.google.de/"),
                    new KeyValuePair<string, string>("err_flag", "0"),
                    new KeyValuePair<string, string>("info_flag", "0"),
                    new KeyValuePair<string, string>("info_msg", "0"),
                    new KeyValuePair<string, string>("username", requiredLogin.Username),
                    new KeyValuePair<string, string>("password", requiredLogin.Password),
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

                var requiredLogin = _config
                    .Logins
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