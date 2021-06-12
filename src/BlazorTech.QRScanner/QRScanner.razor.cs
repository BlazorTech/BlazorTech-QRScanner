using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorTech.QRScanner
{
    public partial class QRScanner : IAsyncDisposable
    {
        [Inject] protected IJSRuntime jSRuntime { get; set; }

        [Parameter] public bool AutoStart { get; set; } = true;
        [Parameter] public int NonUniqueTimeout { get; set; } = 0;
        [Parameter] public EventCallback<string> OnScanReceived { get; set; }
        [Parameter] public RenderFragment CameraUnavailable { get; set; }

        private IJSObjectReference _qrScanner { get; set; }
        private ElementReference _videoElement { get; set; }
        
        
        private readonly DotNetObjectReference<QRScannerCallbackListner> _qrScannerCallbackListnerRef;
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        private bool _hasCameraOnInit = true;
        public QRScanner()
        {
            _qrScannerCallbackListnerRef = DotNetObjectReference.Create(new QRScannerCallbackListner(this));
            moduleTask = new(() => jSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorTech.QRScanner/qr-scanner.umd.min.js").AsTask());
        }

        protected override async Task OnInitializedAsync()
        {
            //To avoid exception when camera exist but access not allowed
            try
            {
                var module = await moduleTask.Value;
                _hasCameraOnInit = await module.InvokeAsync<bool>("deviceHasCamera");
                _qrScanner = await module.InvokeAsync<IJSObjectReference>("createQrScanner", _videoElement, _qrScannerCallbackListnerRef, NonUniqueTimeout);
                if (AutoStart && _hasCameraOnInit)
                    await StartCapturingAsync();
            }
            catch (Exception)
            {
                _hasCameraOnInit = false;
            }
        }

        public async Task StartCapturingAsync()
        {
            await _qrScanner.InvokeVoidAsync("start");
        }

        public async Task StopCapturingAsync()
        {
            await _qrScanner.InvokeVoidAsync("stop");
        }

        public async ValueTask DisposeAsync()
        {
            await _qrScanner.InvokeVoidAsync("destroy");
            _qrScannerCallbackListnerRef.Dispose();
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
            _qrScanner = null;
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
