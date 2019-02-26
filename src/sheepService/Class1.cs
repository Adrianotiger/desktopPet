using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace sheepService
{
    public sealed class Handler : IBackgroundTask
    {
        private BackgroundTaskDeferral _backgroundTaskDeferral;
        private AppServiceConnection _appServiceConnection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //get the task instance deferral
            this._backgroundTaskDeferral = taskInstance.GetDeferral();

            //hooking up to the Canceled event to close app connection
            //taskInstance.Canceled += OnTaskCanceled;

            //getting the AppServiceTriggerDetails and hooking up to the RequestReceived event
            var details = (AppServiceTriggerDetails)taskInstance.TriggerDetails;
            _appServiceConnection = details.AppServiceConnection;
            _appServiceConnection.RequestReceived += OnRequestReceived;
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {

        }
    }
}
