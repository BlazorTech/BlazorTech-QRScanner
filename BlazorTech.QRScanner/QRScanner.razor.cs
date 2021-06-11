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
        private ElementReference _videoElement { get; set; }

        [Parameter]
        public EventCallback<string> OnScanReceived { get; set; }

        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                DotNetObjectReference<QRScanner> objRef = DotNetObjectReference.Create(this);
                var popperWrapper = await jSRuntime.InvokeAsync<IJSInProcessObjectReference>("import", "./_content/BlazorTech.QRScanner/qr-scanner.umd.min.js");
                var qrScanner = await popperWrapper.InvokeAsync<IJSObjectReference>("createQrScanner", _videoElement, objRef);
                await qrScanner.InvokeVoidAsync("start");
            }
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
