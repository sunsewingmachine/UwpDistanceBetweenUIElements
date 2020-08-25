using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using SharedLibrary.Common;
using SharedLibrary.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Shared_AllClasses.Common;
using Shared_AllClasses.ViewModels;
using Shared_AllClasses.Views.Custom;
using Shared_Themes.ViewModels;
using Reg = SharedLibrary.Common.Reg;

namespace SharedLibrary.Views
{
    public sealed partial class LearnModePage : Page
    {
        private int ReadLoopTimes = 1;
        public PicturesPageViewModel ViewModel { get; set; }
		DispatcherTimer timer = new DispatcherTimer();
        public ThemeColor MyTheme { get; set; } = new ThemeColor();
        ObservableCollection<string> repeats = new ObservableCollection<string>();

        public LearnModePage()
		{
            ThemeController.RefreshTheme(MyTheme);
			this.InitializeComponent();
			this.DataContext = ViewModel = new PicturesPageViewModel();
            Loaded += LearnModePage_Loaded;

			inkCanvas.InkPresenter.InputDeviceTypes =
				Windows.UI.Core.CoreInputDeviceTypes.Mouse |
				Windows.UI.Core.CoreInputDeviceTypes.Touch |
				Windows.UI.Core.CoreInputDeviceTypes.Pen;
		}
		
        private bool IsLoading = true;
        private void LearnModePage_Loaded(object sender, RoutedEventArgs e)
		{
			SharedNavigationController.ShowHideGrdLoading(true);
			IsLoading = false;
            SliderImageSize.Value = Reg.GetSetting(Glob.Keys.SliderImageSizeValue, 50);
            LoadCboRepeats();
            CboRepeat.SelectedIndex = Reg.GetSetting(Glob.Keys.CboRepeatSelectedIndex, 0);
            SetImageAndInkCanvasSize();
            TbJustForFocus.Focus(FocusState.Programmatic);
            SharedNavigationController.ShowHideGrdLoading(false);
		}


        public void ShowThemeDialog()
        {
            var myEvent = new ThemeController.MyThemeChangedEvent();
            ThemeController.ShowThemeDialog(MyTheme, myEvent);
        }


		private async void NextHyperLink_Clicked(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
		{
			ViewModel.SetAllPictureItemsAsForward();
			ViewModel.ReadDescription();

			if(ViewModel.NextItem == null && !NextClickedOnNull)
            {
                NextClickedOnNull = true;
				await Task.Delay(5000);
                ViewModel.GoBackLearnPage();
            }
		}

        private bool NextClickedOnNull = false;


   //     private async void ResetNextClickButton()
   //     {
			//await Task.Delay(6000);
			//NextClickedOnNull = false;
   //     }


		private void PrevHyperLink_Clicked(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
		{
			ViewModel.SetAllPictureItemsAsBackward();
			ViewModel.ReadDescription();
		}

		private async void Timer_Tick(object sender, object e)
		{
			ViewModel.SecIncrement++;

			//Exit, If end of the reading-item is reached
			if (ViewModel.PicturesList.Count() - 1 == ViewModel.GetCurrentItemIndex() && 
                ViewModel.SecIncrement >= Constants.AutoGoNextPeriod)
			{
				if (!ViewModel.ReadDescriptionEnabled && !ViewModel.ReadNameEnabled && !ViewModel.ReadLongDescriptionEnabled)
                {
				    timer.Stop();
                    ViewModel.GoBackLearnPage();
                    return;
                }
				
                if ((ViewModel.NextItem == null) && ViewModel.IsMediaEnded)
                {
                    if (ReadLoopTimes > 1)
                    {
                        ReadLoopTimes--;
                        ViewModel.CurrentItem = null;
                        ViewModel.NextItem = null;
                        ViewModel.PrevItem = null;
                        ViewModel.MarkAsFinished();

                        //ViewModel = new PicturesPageViewModel();
                        //ViewModel.Reset();
                        ViewModel.IsMediaEnded = false;
                        ViewModel.SetAllPictureItemsAsForward();
                        ViewModel.ReadDescription();
                        ViewModel.SecIncrement = 0;
                    }
                    else
                    {
                        timer.Stop();
                        ViewModel.GoBackLearnPage();
                        return;
                    }
                }
            }

			//Show next reading-item, if the time reached or media ended
			if (ViewModel.SecIncrement >= Constants.AutoGoNextPeriod)
            {
				var readAnything = ViewModel.ReadNameEnabled || ViewModel.ReadDescriptionEnabled || ViewModel.ReadLongDescriptionEnabled;
                
				if ((readAnything && ViewModel.IsMediaEnded) || (!readAnything && ViewModel.AutoGoNextEnabled))
                {
                    timer.Stop();
                    ViewModel.PrevItem = null;
                    mainImage.E3StartEnterScaledAnimation(GetExitAniScaled());
                    await Task.Delay(AniTime);

                    ViewModel.IsMediaEnded = false;
					ViewModel.SetAllPictureItemsAsForward();
					ViewModel.ReadDescription();
					ViewModel.SecIncrement = 0;

                    mainImage.E3StartEnterScaledAnimation(GetEnterAniScaled());
                    var next = ViewModel.NextItem;
                    ViewModel.NextItem = null;
                    await Task.Delay(AniTime);
                    ViewModel.NextItem = next;
                    timer.Start();
                }
			}
        }

        private int AniTime = 300;
        private ExtensionsAnimations.EnterExitScaled GetEnterAniScaled()
        {
            //var transform = mainImage.TransformToVisual(ImgNext);
            //var kk = transform.TransformPoint(new Point(0,0));
            //var distance = (int) kk.X;
            var distance = -1300;

            var arg = new ExtensionsAnimations.EnterExitScaled();
            arg.IsScaleEnabled = true;
            arg.Axis = ExtensionsAnimations.AnimationAxis.X;
            arg.Direction = ExtensionsAnimations.AnimationDirection.In;
            arg.Distance = distance;
            arg.StartScale = 0.3;
            arg.EndScale = 1;
            arg.Time = AniTime;
            return arg;
        }

        private ExtensionsAnimations.EnterExitScaled GetExitAniScaled()
        {
            //var transform = mainImage.TransformToVisual(ImgPrev);
            //var kk = transform.TransformPoint(new Point(0, 0));
            //var distance = (1 * ((int) kk.X));
            var distance = 850;

            var arg = new ExtensionsAnimations.EnterExitScaled();
            arg.IsScaleEnabled = true;
            arg.Axis = ExtensionsAnimations.AnimationAxis.X;
            arg.Direction = ExtensionsAnimations.AnimationDirection.Out;
            arg.Distance = distance;
            arg.StartScale = 1;
            arg.EndScale = 0.3;
            arg.Time = AniTime;
            return arg;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			timer.Stop();
			ViewModel.StopAudio();
		}

		private void AutoGoNext_Changed(object sender, RoutedEventArgs e)
		{
            if (!(sender is CheckBox checkbox)) return;

            if (checkbox.IsChecked != null && (bool)checkbox.IsChecked)
            {
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else
            {
                timer.Stop();
                ViewModel.SecIncrement = 0;
            }
        }

		private async void inkCanvas_SaveClick(object sender, RoutedEventArgs e)
		{
            var playerName = LevelPageViewModel.GetCurrentPlayer();
            var subName = LevelPageViewModel.GetSelectedSubLevelName();
            var imgName = ViewModel.CurrentItem.ImagePath.Split("\\").Last();
            var path = Path.Combine(playerName, subName, imgName);

			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			StorageFile file = await storageFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);
			IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

			using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
			{
				await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
				await outputStream.FlushAsync();
			}
			stream.Dispose();
		}

		private async void OnImageOpened(object sender, RoutedEventArgs e)
		{
            SetImageAndInkCanvasSize();
			string playerName = LevelPageViewModel.GetCurrentPlayer();
			string subName = LevelPageViewModel.GetSelectedSubLevelName();
			string imgName = ViewModel.CurrentItem.ImagePath.Split("\\").Last();
            string path = Path.Combine(playerName, subName, imgName);

			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			
			if (File.Exists(Path.Combine(storageFolder.Path,path)))
			{
				StorageFile file = await storageFolder.GetFileAsync(path);
				IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

				using (var inputStream = stream.GetInputStreamAt(0))
				{
					await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
				}
				stream.Dispose();
			}
			else
			{
				inkCanvas.InkPresenter.StrokeContainer.Clear();
			}
		}

		private void OptionsPage_Click(object sender, RoutedEventArgs e)
		{
			Glob.MainFrame.Navigate(typeof(OptionsPage));
		}

        private void StkNextItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            NextHyperLink_Clicked(null, null);
        }

        private void StkPrevItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            PrevHyperLink_Clicked(null, null);
        }

        private void LearnModePage_OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
            SetImageAndInkCanvasSize();
        }

        private void SliderImageSize_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (IsLoading) return;
            SetImageAndInkCanvasSize();
            Reg.SaveSetting(Glob.Keys.SliderImageSizeValue, SliderImageSize.Value);
        }
        private void CboRepeat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading) return;
            Reg.SaveSetting(Glob.Keys.CboRepeatSelectedIndex, CboRepeat.SelectedIndex);
            FindReadLoopTimes();
        }

        private void FindReadLoopTimes()
        {
            int selindex = CboRepeat.SelectedIndex;
            switch (selindex)
            {
                case -1:
                    ReadLoopTimes = 1;
                    break;

                case 0:
                    ReadLoopTimes = 1;
                    break;

                case 1:
                    ReadLoopTimes = 2;
                    break;

                case 2:
                    ReadLoopTimes = 3;
                    break;

                case 3:
                    ReadLoopTimes = 4;
                    break;

                case 4:
                    ReadLoopTimes = 5;
                    break;

                case 5:
                    ReadLoopTimes = 10;
                    break;

                case 6:
                    ReadLoopTimes = 1000;
                    break;

                default:
                    ReadLoopTimes = 0;
                    break;
            }
        }

        private void LoadCboRepeats()
        {
            repeats.Add("Repeat 1 time");
            repeats.Add("Repeat 2 times");
            repeats.Add("Repeat 3 times");
            repeats.Add("Repeat 4 times");
            repeats.Add("Repeat 5 times");
            repeats.Add("Repeat 10 times");
            repeats.Add("Repeat Forever");
            CboRepeat.ItemsSource = repeats;

        }

        private void SetImageAndInkCanvasSize()
        {
			var width = (2 * 800) * (SliderImageSize.Value / 100);
            var height = (2 * 450) * (SliderImageSize.Value / 100);

            ViewboxMain.Width = width;
            ViewboxMain.Height = height;
        }


        private void LearnModePage_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape || e.Key == VirtualKey.Number0 || e.Key == VirtualKey.NumberPad0 || e.Key == VirtualKey.Left)
            {
                if (Glob.MainFrame.CanGoBack) Glob.MainFrame.GoBack();
            }
            else if (e.Key == VirtualKey.N || e.Key == VirtualKey.Right || e.Key == VirtualKey.NumberPad2)
            {
                StkNextItem_OnTapped(null, null);
            }
            else if (e.Key == VirtualKey.P || e.Key == VirtualKey.Left || e.Key == VirtualKey.NumberPad1)
            {
                StkPrevItem_OnTapped(null, null);
			}
			else if (e.Key == VirtualKey.F || e.Key == VirtualKey.F11)
            {
                AllGlob.ToggleFullscreen();
			}
			else if (e.Key == VirtualKey.F5)
            {
                BtnDeveloper_Click(null, null);
			}
		}

        private async void BtnDeveloper_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputBoxDialog();
            await dialog.ShowAsync();
            if (dialog.Result == null) return;
            var command = dialog.Result.ToLower();

            switch (command)
            {
                case "done":
                    timer.Stop();
                    ViewModel.NextItem = null;
                    ViewModel.GoBackLearnPage();
                    break;

                case "notdone":
                    timer.Stop();
                    ViewModel.NextItem = null;
                    var playerName = LevelPageViewModel.GetCurrentPlayer();
                    var level = LevelPageViewModel.GetSelectedLevel();
                    var joinedWord = string.Concat(playerName, ".", level, ".", Constants.LearnPage, ".", Constants.LevelCompleted);
                    Reg.SaveSetting(joinedWord, false);
                    Glob.MainFrame.GoBack();
                    break;

                case "activate":
                    //var activation = new Activation();
                    //var questionString = activation.GetQuestionString();
                    //dialog.Title = "Client-id: " + questionString;
                    //dialog.ShowValue = questionString;
                    //await dialog.ShowAsync();
                    //await activation.Activate(dialog.Result);
                    break;

                case "cmdhelp":
                case "showhelp":
                    await Glob.MsgBox_Async(
                        "Warning: Improper usage will lead to failure of the app!" + Environment.NewLine +
                        "Activate +5->*5->Pxx/sun/55786, deactivate/no55786" + Environment.NewLine +
                        "isfreeforpromotion=true, isfreeforpromotion=false, " + Environment.NewLine +
                        "cmd0/ratingnotdone, cmd5/ratingdone, xxx/del/deletesettings/delall, " + Environment.NewLine +
                        "cmd6/easy/completefalse");
                    break;

                case "isfreeforpromotion=false":
                case "cmd9=false":
                    //Reg.SaveSetting(Glob.Settings.IsFreeForPromotion, false);
                    await Glob.MsgBox_Async("IsFreeForPromotion = false");
                    break;

                case "isfreeforpromotion=true":
                case "cmd9=true":
                    //Reg.SaveSetting(Glob.Settings.IsFreeForPromotion, true);
                    await Glob.MsgBox_Async("IsFreeForPromotion = true");
                    break;

                case "deactivate":
                case "no55786":
                    //Reg.SaveSetting(AppData.Addon100Lessons, false);
                    //Reg.SaveSetting(AppData.Addon200Lessons, false);
                    //MyConstants.IsPro200 = MyConstants.IsPro100 = false;
                    await Glob.MsgBox_Async("Successfully De-Activated! Restart the App if required.");
                    break;

                case "ratingdone":
                case "cmd5":
                    //Reg.SaveSetting(Glob.Settings.Rating, 5);
                    //Reg.SaveSetting(Glob.Settings.hide_count, 100);
                    //Reg.SaveSetting(Glob.Settings.SimpleRatingDone, true);
                    await Glob.MsgBox_Async("Done!");
                    break;

                case "ratingnotdone":
                case "cmd0":
                    //Reg.SaveSetting(Glob.Settings.Rating, 0);
                    //Reg.SaveSetting(Glob.Settings.hide_count, 100);
                    //Reg.SaveSetting(Glob.Settings.SimpleRatingDone, false);
                    await Glob.MsgBox_Async("Done!");
                    break;

                case "delall":
                case "deletesettings":
                    await ApplicationData.Current.ClearAsync();
                    CoreApplication.Exit();
                    break;

                case "del":
                case "xxx":
                    await Reg.DeleteAllSettings(true);
                    await ApplicationData.Current.ClearAsync();
                    CoreApplication.Exit();
                    break;

                case "completefalse":
                case "cmd6":
                case "easy":
                    //Reg.SaveSetting(Glob.Settings.CompletePrevious, false);
                    //bCompletePrevious = false;
                    //await Glob.MsgBox_Async("Done! CompletePrevious = false");
                    break;

                case "nocompletefalse":
                case "nocmd6":
                case "noeasy":
                    //Reg.SaveSetting(Glob.Settings.CompletePrevious, true);
                    //bCompletePrevious = true;
                    //await Glob.MsgBox_Async("Easy mode activated: Navigate to any lesson now");
                    break;

                case "sun": //Use it for Emergency
                case "55786": //Use it for Emergency
                case "checkpurchase": //Use it for Emergency
                    //Reg.SaveSetting(AppData.Addon200Lessons, true);
                    //Reg.SaveSetting(Glob.Settings.GivenFreeForActivationCode, true);
                    //MyConstants.IsPro200 = true;
                    //await Glob.MsgBox_Async("Successfully Activated!  Thanks for purchasing. Restart the App if required.");
                    break;


                case "sun-easy": //Use it for Emergency
                case "55786-easy": //Use it for Emergency
                case "checkpurchase-easy": //Use it for Emergency
                    //Reg.SaveSetting(AppData.Addon200Lessons, true);
                    //Reg.SaveSetting(Glob.Settings.GivenFreeForActivationCode, true);
                    //MyConstants.IsPro200 = true;
                    //Reg.SaveSetting(Glob.Settings.CompletePrevious, false);
                    //bCompletePrevious = false;
                    //await Glob.MsgBox_Async("Successfully Activated!  Thanks for purchasing. Restart the App if required." +
                    //                        Environment.NewLine + "(Easy mode activated: Navigate to any lesson)");


                    break;


                default:
                    break;
            }
        }

    }
}
