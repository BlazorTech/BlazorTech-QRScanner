using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorTech.QRScanner
{
    public partial class QRScanner : IDisposable
    {
        [Inject] protected IJSRuntime jSRuntime { get; set; }

        [Parameter] public bool AutoStart { get; set; } = true;
        [Parameter] public int NonUniqueTimeout { get; set; } = 0;
        [Parameter] public EventCallback<string> OnScanReceived { get; set; }
        
        
        private IJSObjectReference _qrScanner { get; set; }
        private ElementReference _videoElement { get; set; }
        
        
        private readonly DotNetObjectReference<QRScannerCallbackListner> _qrScannerCallbackListnerRef;
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public QRScanner()
        {
            _qrScannerCallbackListnerRef = DotNetObjectReference.Create(new QRScannerCallbackListner(this));
            moduleTask = new(() => jSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorTech.QRScanner/qr-scanner.umd.min.js").AsTask());
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var popperWrapper = await moduleTask.Value;
                _qrScanner = await popperWrapper.InvokeAsync<IJSObjectReference>("createQrScanner", _videoElement, _qrScannerCallbackListnerRef, NonUniqueTimeout);
                if(AutoStart)
                    await StartCapturing();
            }
        }

        public async Task StartCapturing()
        {
            await _qrScanner.InvokeVoidAsync("start");
        }

        public async Task StopCapturing()
        {
            await _qrScanner.InvokeVoidAsync("stop");
        }

        public void Dispose()
        {
            _qrScannerCallbackListnerRef.Dispose();
        }

        protected class QRScannerCallbackListner
        {
            protected readonly QRScanner _qrScanner;
            public QRScannerCallbackListner(QRScanner qRScanner)
            {
                _qrScanner = qRScanner;
            }
            [JSInvokable("SendCodeAsync")]
            public async Task SendCodeAsync(string qr)
            {
                await _qrScanner.OnScanReceived.InvokeAsync(qr);
            }
        }
        
    }
}
