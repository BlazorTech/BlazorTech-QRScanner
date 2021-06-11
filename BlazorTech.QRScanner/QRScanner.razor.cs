using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTech.QRScanner
{
    public partial class QRScanner
    {
        [Inject] protected IJSRuntime jSRuntime { get; set; }

        [Parameter] public bool AutoStart { get; set; } = true;
        [Parameter] public EventCallback<string> OnScanReceived { get; set; }
        
        
        private IJSObjectReference _qrScanner { get; set; }

        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private ElementReference _videoElement { get; set; }

        public QRScanner()
        {
            moduleTask = new(() => jSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorTech.QRScanner/qr-scanner.umd.min.js").AsTask());
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                DotNetObjectReference<QRScanner> objRef = DotNetObjectReference.Create(this);
                var popperWrapper = await moduleTask.Value;
                _qrScanner = await popperWrapper.InvokeAsync<IJSObjectReference>("createQrScanner", _videoElement, objRef);
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

        [JSInvokable("SendCodeAsync")]
        public async Task SendCodeAsync(string qr)
        {
            Console.WriteLine($"Scanned:{qr}");
            await OnScanReceived.InvokeAsync(qr);
        }

        public class QRScannerCallbackListner
        {
            
        }
        
    }
}
