using System.Net;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace Listener_Core
{
    public interface IFunc
    {
        void OnGetUrl(string url);
    }

    public class Core
    {
        private ProxyServer? _proxyServer;

        private IFunc icb;

        public void TitaniumProxy(IFunc cb)
        {
            icb = cb;
            if (_proxyServer != null)
            {
                return;
            }

            _proxyServer = new ProxyServer();
            _proxyServer.CertificateManager.SaveFakeCertificates = true;

            _proxyServer.BeforeRequest += OnRequest;
            _proxyServer.BeforeResponse += OnResponse;
            _proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            _proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;


            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);

            explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;

            _proxyServer.AddEndPoint(explicitEndPoint);
            _proxyServer.Start();

            _proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
            _proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);


            explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
            _proxyServer.BeforeRequest += OnRequest;
            _proxyServer.BeforeResponse += OnResponse;
            _proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            _proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;
        }

        public void Stop()
        {
            _proxyServer?.Stop();
            _proxyServer = null;
        }

        public bool IsRunning()
        {
            return _proxyServer == null ? false : _proxyServer.ProxyRunning;
        }

        private async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            e.DecryptSsl = true;
        }

        public async Task OnRequest(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url.Contains("/api/getGachaLog"))
            {
                icb.OnGetUrl(e.HttpClient.Request.Url);
            }
        }

        public async Task OnResponse(object sender, SessionEventArgs e)
        {
        }

        public Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            // set IsValid to true/false based on Certificate Errors
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                e.IsValid = true;

            return Task.CompletedTask;
        }

        public Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}