using Chadder.Data.Response;
using Chadder.Data.Util;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Util
{
    public class SignalRConnection
    {
        public string Domain { get; private set; }
        public string Token { get; set; }
        public SignalRConnection(string d)
        {
            Domain = d;
            CreateHubConnection();
        }
        protected virtual void CreateHubConnection()
        {
            HubConnection = new HubConnection(Domain);
            HubConnection.Closed += OnHubDisconnected;
        }

        public bool ShouldBeConnected { get; set; }
        protected HubConnection HubConnection { get; set; }

        public Task<T> Invoke<T>(IHubProxy proxy, string method, params object[] data) where T : BasicResponse, new()
        {
            return Invoke<T>(proxy, method, 2, 1000, data);
        }
        public async Task<T> Invoke<T>(IHubProxy proxy, string method, int retries, int nextRetry, params object[] data) where T : BasicResponse, new()
        {
            Exception Inner = null;
            do
            {
                try
                {
                    if (HubConnection != null && proxy != null && HubConnection.State == ConnectionState.Connected)
                    {
                        return await proxy.Invoke<T>(method, data);
                    }
                }
                catch (Exception ex)
                {
                    Inner = ex;
                    Insight.Track(string.Format("Invoke {0} Failed: {1}", method, HubConnection.State.ToString()));
                    if (HubConnection.State == ConnectionState.Connected)
                    {
                        Insight.Report(ex);
                    }
                }
                if (retries > 0)
                {
                    await Task.Delay(nextRetry);
                    nextRetry *= 2;
                    retries--;
                }
                else
                    break;
            } while (true);
            var response = new T();
            response.InnerException = Inner;
            response.Error = ChadderError.CONNECTION_ERROR;
            return response;
        }

        protected ExponentialBackoff ConnectTimer = new ExponentialBackoff(1000, 10000);
        /// <summary>
        /// Called when [hub disconnected].
        /// </summary>
        private async void OnHubDisconnected()
        {
            Insight.Track("OnHubDisconnected");
            if (ShouldBeConnected)
            {
                await ConnectTimer.Failed();
                await Connect();
            }
        }

        /// <summary>
        /// Connects the SignalR Hub
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Connect()
        {
            try
            {
                ShouldBeConnected = true;
                var holder = Insight.StartTimer("Connection timer");
                if (Token != null)
                    HubConnection.Headers["Authorization"] = "Bearer " + Token;
                await HubConnection.Start();
                Insight.StopTimer(holder);
                Insight.Track("HUB - Connected using " + HubConnection.Transport.Name);
                ConnectTimer.Reset();
                return true;
            }
            catch (HttpClientException ex)
            {
                Insight.Report(ex);
                ex.Response.Content.ReadAsStringAsync().ContinueWith((r) =>
                {
                    Insight.Track(r.Result);
                });
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// Disconnects SignalR. if set is online is true it will not try to reconnect until Connect is called.
        /// Calling ReconnectIfNeeded after disconnect(false) will try to connect
        /// </summary>
        /// <param name="setIsOnline"></param>
        public void Disconnect(bool setIsOnline = true)
        {
            if (HubConnection != null)
            {
                var c = HubConnection;
                HubConnection = null;
                CreateHubConnection();
                c.Closed -= OnHubDisconnected;
                Task.Run(() => { try { c.Stop(); } catch { } });
            }

            if (setIsOnline)
                ShouldBeConnected = false;

        }

        public bool IsConnected()
        {
            return HubConnection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected;
        }

        static public void FirstHubConnection(string url)
        {
            Task.Run(async () =>
            {
                try
                {
                    var holder = Insight.StartTimer("First SignalR connection");
                    var hubConnection = new HubConnection(url);
                    await hubConnection.Start();
                    Insight.StopTimer(holder);
                    hubConnection.Stop();
                }
                catch
                {

                }
            });
        }
    }
}
