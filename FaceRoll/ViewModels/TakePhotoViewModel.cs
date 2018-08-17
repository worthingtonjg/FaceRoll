using DataAccess;
using FaceRoll.Common;
using FaceRoll.Model;
using FaceRoll.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace FaceRoll.ViewModels
{
    public class TakePhotoViewModel : ViewModelBase
    {
        private bool _isPreviewing;
        private DisplayRequest _displayRequest = new DisplayRequest();
        private CaptureElement _captureElement;
        private Canvas _faceCanvas;
        private FaceHelper _faceHelper;
        private CoreDispatcher _dispatcher;
        private MediaCapture _mediaCapture;
        private DispatcherTimer _timer;
        private string _defaultInstructions = "Analyzing face...";
        private FaceDetectionEffect _faceDetectionEffect;
        private DisplayOrientations _displayOrientation = DisplayOrientations.Landscape;
        private int _faceBoxSize;

        public TakePhotoViewModel()
        {
            SetValue(() => Instructions, _defaultInstructions);

            _faceBoxSize = int.Parse(SettingsHelper.ReadSettings(SettingsHelper.FaceBoxSize));

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, int.Parse(SettingsHelper.ReadSettings(SettingsHelper.TimeBetweenPhotos)));
            _timer.Tick += _timer_Tick; ;
        }

        [XamlProperty]
        public string Instructions { get; set; }

        [XamlProperty]
        public ICommand BackCommand
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    _timer.Stop();
                    NavigationHelper.Navigate(typeof(ManageMeetingsPage));
                });
            }
        }

        public async void Init(CoreDispatcher coreDispatcher, CaptureElement element, Canvas faceCanvas)
        {
            _dispatcher = coreDispatcher;
            _captureElement = element;
            _faceCanvas = faceCanvas;

            _faceHelper = new FaceHelper(
                SettingsHelper.ReadSettings(SettingsHelper.FaceApiSubscriptionKey),
                SettingsHelper.ReadSettings(SettingsHelper.FaceApiRoot));

            await _faceHelper.InitFaceGroup(SettingsHelper.ReadSettings(SettingsHelper.FaceApiPersonGroup));
        }

        public async Task StartPreviewAsync()
        {
            try
            {
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();

                _displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException ex)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                var dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();

                return;
            }

            try
            {
                _captureElement.Source = _mediaCapture;
                await _mediaCapture.StartPreviewAsync();
                await CreateFaceDetectionEffectAsync();

                _isPreviewing = true;
            }
            catch (System.IO.FileLoadException)
            {
                _mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }

        }

        public async Task CleanupCameraAsync()
        {
            try
            {
                if (_mediaCapture != null)
                {
                    if (_isPreviewing)
                    {
                        await _mediaCapture.StopPreviewAsync();
                    }

                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _captureElement.Source = null;

                        if (_displayRequest != null)
                        {
                            _displayRequest.RequestRelease();
                        }

                        _mediaCapture.Dispose();
                        _mediaCapture = null;
                    });
                }
            }
            catch (Exception ex)
            {
                // Eat the message
                Debug.WriteLine(ex.Message);
            }

        }

        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                var dialog = new MessageDialog("The camera preview can't be displayed because another app has exclusive access");
                await dialog.ShowAsync();
            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !_isPreviewing)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await StartPreviewAsync();
                });
            }
        }

        private async Task CreateFaceDetectionEffectAsync()
        {
            // Create the definition, which will contain some initialization settings
            var definition = new FaceDetectionEffectDefinition();

            // To ensure preview smoothness, do not delay incoming samples
            definition.SynchronousDetectionEnabled = false;

            // In this scenario, choose detection speed over accuracy
            definition.DetectionMode = FaceDetectionMode.HighPerformance;

            // Add the effect to the preview stream
            _faceDetectionEffect = (FaceDetectionEffect)await _mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview);

            // Register for face detection events
            _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;

            // Choose the shortest interval between detection events
            _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(300);

            // Start detecting faces
            _faceDetectionEffect.Enabled = true;
        }

        private async void FaceDetectionEffect_FaceDetected(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
            if (!_isPreviewing) return;


            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                SetValue(() => Instructions, "Please move closer until the box turns red...");

                if (HighlightDetectedFaces(args.ResultFrame.DetectedFaces))
                {
                    SetValue(() => Instructions, _defaultInstructions);

                    _faceDetectionEffect.FaceDetected -= FaceDetectionEffect_FaceDetected;

                    try
                    {
                        StorageFile file = await GetPhotoFromPreviewFrame();

                        if (file != null)
                        {
                            List<Identification> matches = await AnalyzePhoto(file);

                            _faceCanvas.Children.Clear();

                            if (matches.Count > 0)
                            {
                                if (matches.First().Person.Name.ToLower() == "unknown")
                                {
                                    ClipboardHelper clipboardHelper = new ClipboardHelper();
                                    clipboardHelper.ImageToClipboard(file);
                                    NavigationHelper.Navigate(typeof(FaceNotFoundPage), false);
                                }
                                else
                                {
                                    bool alreadyAttending = await Repository.AlreadyAttending(App.ActiveMeeting.MeetingId, matches.First().Person.Name);
                                    if (!alreadyAttending)
                                    {
                                        NavigationHelper.Navigate(typeof(FaceFoundPage), matches.First());
                                        ClipboardHelper clipboardHelper = new ClipboardHelper();
                                        clipboardHelper.ImageToClipboard(file);
                                    }
                                    else
                                    {

                                        SetValue(() => Instructions, $"{matches.First().Person.Name} - You are already on the roll.  Please take a seat.");

                                        _timer.Start();
                                    }
                                }
                            }
                            else
                            {
                                _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;
                    }
                }
            });
        }

        private void _timer_Tick(object sender, object e)
        {
            _timer.Stop();
            _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;
        }

        private async Task<StorageFile> GetPhotoFromPreviewFrame()
        {
            var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

            var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

            var frame = await _mediaCapture.GetPreviewFrameAsync(videoFrame);

            SoftwareBitmap frameBitmap = frame.SoftwareBitmap;

            WriteableBitmap bitmap = new WriteableBitmap(frameBitmap.PixelWidth, frameBitmap.PixelHeight);

            frameBitmap.CopyToBuffer(bitmap.PixelBuffer);

            var myPictures = await StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            StorageFile file = await myPictures.SaveFolder.CreateFileAsync("_photo.jpg", CreationCollisionOption.ReplaceExisting);

            using (var captureStream = new InMemoryRandomAccessStream())
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await bitmap.ToStreamAsJpeg(captureStream);

                    var decoder = await BitmapDecoder.CreateAsync(captureStream);
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(fileStream, decoder);

                    var properties = new BitmapPropertySet {
                            { "System.Photo.Orientation", new BitmapTypedValue(PhotoOrientation.Normal, PropertyType.UInt16) }
                        };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);

                    await encoder.FlushAsync();
                }
            }

            // Close the frame
            frame.Dispose();
            frame = null;
            return file;
        }

        private async Task<List<Model.Identification>> AnalyzePhoto(StorageFile file)
        {
            var clipboardHelper = new ClipboardHelper();

            var personGroupId = SettingsHelper.ReadSettings(SettingsHelper.FaceApiPersonGroup);

            var matches = await _faceHelper.Identify(personGroupId, file);

            return matches;
        }

        private bool HighlightDetectedFaces(IReadOnlyList<DetectedFace> faces)
        {
            var faceDetected = false;

            // Remove any existing rectangles from previous events
            _faceCanvas.Children.Clear();


            // For each detected face
            for (int i = 0; i < faces.Count; i++)
            {
                // Face coordinate units are preview resolution pixels, which can be a different scale from our display resolution, so a conversion may be necessary
                Rectangle faceBoundingBox = ConvertPreviewToUiRectangle(faces[i].FaceBox);

                // Set bounding box stroke properties
                faceBoundingBox.StrokeThickness = 2;
                var brush = new SolidColorBrush(Colors.Blue);
                if (faces[0].FaceBox.Width > _faceBoxSize)
                {
                    brush = new SolidColorBrush(Colors.Red);

                    faceDetected = true;
                }


                // Highlight the first face in the set
                faceBoundingBox.Stroke = (i == 0 ? brush : new SolidColorBrush(Colors.DeepSkyBlue));

                // Add grid to canvas containing all face UI objects
                _faceCanvas.Children.Add(faceBoundingBox);
            }

            // Update the face detection bounding box canvas orientation
            SetFacesCanvasRotation();

            return faceDetected;
        }

        private void SetFacesCanvasRotation()
        {
            // Calculate how much to rotate the canvas
            int rotationDegrees = ConvertDisplayOrientationToDegrees(_displayOrientation);

            // The rotation direction needs to be inverted if the preview is being mirrored, just like in SetPreviewRotationAsync
            //if (_mirroringPreview)
            //{
            //rotationDegrees = (360 - rotationDegrees) % 360;
            //}

            // Apply the rotation
            var transform = new RotateTransform { Angle = rotationDegrees };
            _faceCanvas.RenderTransform = transform;


            var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
            var previewArea = GetPreviewStreamRectInControl(previewProperties, _captureElement);

            // For portrait mode orientations, swap the width and height of the canvas after the rotation, so the control continues to overlap the preview
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                _faceCanvas.Width = previewArea.Height;
                _faceCanvas.Height = previewArea.Width;

                // The position of the canvas also needs to be adjusted, as the size adjustment affects the centering of the control
                Canvas.SetLeft(_faceCanvas, previewArea.X - (previewArea.Height - previewArea.Width) / 2);
                Canvas.SetTop(_faceCanvas, previewArea.Y - (previewArea.Width - previewArea.Height) / 2);
            }
            else
            {
                _faceCanvas.Width = previewArea.Width;
                _faceCanvas.Height = previewArea.Height;

                Canvas.SetLeft(_faceCanvas, previewArea.X);
                Canvas.SetTop(_faceCanvas, previewArea.Y);
            }

            // Also mirror the canvas if the preview is being mirrored
            //_faceCanvas.FlowDirection = FlowDirection.RightToLeft;
            //_faceCanvas.FlowDirection = FlowDirection.LeftToRight;
            //_faceCanvas.FlowDirection = _mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                case DisplayOrientations.Landscape:
                default:
                    return 0;
            }
        }

        public Rect GetPreviewStreamRectInControl(VideoEncodingProperties previewResolution, CaptureElement previewControl)
        {
            var result = new Rect();

            // In case this function is called before everything is initialized correctly, return an empty result
            if (previewControl == null || previewControl.ActualHeight < 1 || previewControl.ActualWidth < 1 ||
                previewResolution == null || previewResolution.Height == 0 || previewResolution.Width == 0)
            {
                return result;
            }

            var streamWidth = previewResolution.Width;
            var streamHeight = previewResolution.Height;

            // For portrait orientations, the width and height need to be swapped
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                streamWidth = previewResolution.Height;
                streamHeight = previewResolution.Width;
            }

            // Start by assuming the preview display area in the control spans the entire width and height both (this is corrected in the next if for the necessary dimension)
            result.Width = previewControl.ActualWidth;
            result.Height = previewControl.ActualHeight;

            // If UI is "wider" than preview, letterboxing will be on the sides
            if ((previewControl.ActualWidth / previewControl.ActualHeight > streamWidth / (double)streamHeight))
            {
                var scale = previewControl.ActualHeight / streamHeight;
                var scaledWidth = streamWidth * scale;

                result.X = (previewControl.ActualWidth - scaledWidth) / 2.0;
                result.Width = scaledWidth;
            }
            else // Preview stream is "wider" than UI, so letterboxing will be on the top+bottom
            {
                var scale = previewControl.ActualWidth / streamWidth;
                var scaledHeight = streamHeight * scale;

                result.Y = (previewControl.ActualHeight - scaledHeight) / 2.0;
                result.Height = scaledHeight;
            }

            return result;
        }

        private Rectangle ConvertPreviewToUiRectangle(BitmapBounds faceBoxInPreviewCoordinates)
        {
            var result = new Rectangle();
            var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

            // If there is no available information about the preview, return an empty rectangle, as re-scaling to the screen coordinates will be impossible
            if (previewProperties == null) return result;

            // Similarly, if any of the dimensions is zero (which would only happen in an error case) return an empty rectangle
            if (previewProperties.Width == 0 || previewProperties.Height == 0) return result;

            double streamWidth = previewProperties.Width;
            double streamHeight = previewProperties.Height;

            // For portrait orientations, the width and height need to be swapped
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                streamHeight = previewProperties.Width;
                streamWidth = previewProperties.Height;
            }

            // Get the rectangle that is occupied by the actual video feed
            var previewInUI = GetPreviewStreamRectInControl(previewProperties, _captureElement);

            // Scale the width and height from preview stream coordinates to window coordinates
            result.Width = (faceBoxInPreviewCoordinates.Width / streamWidth) * previewInUI.Width;
            result.Height = (faceBoxInPreviewCoordinates.Height / streamHeight) * previewInUI.Height;

            // Scale the X and Y coordinates from preview stream coordinates to window coordinates
            var x = (faceBoxInPreviewCoordinates.X / streamWidth) * previewInUI.Width;
            var y = (faceBoxInPreviewCoordinates.Y / streamHeight) * previewInUI.Height;
            Canvas.SetLeft(result, x);
            Canvas.SetTop(result, y);

            return result;
        }
    }
}
