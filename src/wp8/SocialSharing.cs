using Microsoft.Phone.Tasks;

using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;

namespace Cordova.Extension.Commands
{
    public class SocialSharing : BaseCommand
    {
        #region Public methods

        public void available(string jsonArgs)
        {
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
        }

        public void share(string jsonArgs)
        {

            var options = JsonHelper.Deserialize<string[]>(jsonArgs);

            var message = options[0];
            var title = options[1];
            var files = JsonHelper.Deserialize<string[]>(options[2]);
            var link = options[3];

            if (!"null".Equals(link))
            {
                ShareLinkTask shareLinkTask = new ShareLinkTask();
                shareLinkTask.Title = title;
                shareLinkTask.LinkUri = new System.Uri(link, System.UriKind.Absolute);
                shareLinkTask.Message = message;
                shareLinkTask.Show();
            }
            else if (files != null && files.Length > 0)
            {
                ShareLinkTask shareLinkTask = new ShareLinkTask();
                shareLinkTask.Title = title;
                shareLinkTask.LinkUri = new System.Uri(files[0], System.UriKind.Absolute);
                shareLinkTask.Message = message;
                shareLinkTask.Show();
            }
            else
            {
                var shareStatusTask = new ShareStatusTask { Status = message };
                shareStatusTask.Show();
            }
            // unfortunately, there is no way to tell if something was shared, so just invoke the successCallback
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
        }

        public void shareScreenshot()
        {
            string fileName = string.Format("MoodyBaby_{0:}.jpg", DateTime.Now.Ticks);
            UIElement content;
            if (GetContentRoot(out content))
            {
                var currentScreenImage = new WriteableBitmap((int)content.ActualWidth, (int)content.ActualHeight);
                currentScreenImage.Render(content, new MatrixTransform());
                currentScreenImage.Invalidate();
                SaveToMediaLibrary(currentScreenImage, fileName, 100);
                MessageBox.Show("Captured image " + fileName + " Saved Sucessfully", "WP Capture Screen", MessageBoxButton.OK);

                var shareMediaTask = new ShareMediaTask();
                shareMediaTask.FilePath = fileName;
                shareMediaTask.Show();
                DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
            }

            DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR));
        }

        public void canShareViaEmail(string jsonArgs)
        {
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
        }

        // HTML and attachments are currently not supported on WP8
        public void shareViaEmail(string jsonArgs)
        {
            var options = JsonHelper.Deserialize<string[]>(jsonArgs);
            EmailComposeTask draft = new EmailComposeTask();
            draft.Body = options[0];
            draft.Subject = options[1];
            if (!"null".Equals(options[2]))
            {
                draft.To = string.Join(",", options[2]);
            }
            if (!"null".Equals(options[3]))
            {
                draft.Cc = string.Join(",", options[3]);
            }
            if (!"null".Equals(options[4]))
            {
                draft.Bcc = string.Join(",", options[4]);
            }
            draft.Show();
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK, true));
        }


        #endregion

        #region Private methods

        private bool TryGetContentRoot(out UIElement content)
        {
            content = null;
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                var page = frame.Content as PhoneApplicationPage;
                if (page != null)
                {
        			var view = page.FindName("CordovaView") as CordovaView;
                    if (view != null)
                    {
                        content = view;
                        return true;
                    }
                }
            }
            return false;
        }

        private void SaveToMediaLibrary(WriteableBitmap bitmap, string name, int quality)
        {
            using (var stream = new MemoryStream())
            {
                // Save the picture to the Windows Phone media library.
                bitmap.SaveJpeg(stream, bitmap.PixelWidth, bitmap.PixelHeight, 0, quality);
                stream.Seek(0, SeekOrigin.Begin);
                new MediaLibrary().SavePicture(name, stream);
            }
        }

        #endregion
    }
}