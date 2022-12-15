using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KeyShark;
using KeyShark.Native;
using Newtonsoft.Json;
using PushToMute.Core.Services;
using System.Text;

namespace PushToMute.Core.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        #region Observable Properties and Relay Commands
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PttKeybindAsString))]
        private Keybind? pttKeybind;

        public string PttKeybindAsString
        {
            get
            {
                if (PttKeybind == null || PttKeybind.KeyCodes == null)
                    return "[ None ]";

                var stringBuilder = new StringBuilder();

                foreach (var keyCode in PttKeybind.KeyCodes)
                    stringBuilder.Append($"   {keyCode}");

                return stringBuilder.ToString().Trim().Replace("   ", " + ");
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VolumeReductionPercentageString))]
        private int volumeReductionPercentage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PttKeybind))]
        private bool awaitingHotkeyPress;

        public string VolumeReductionPercentageString => $"{VolumeReductionPercentage}% reduction";


        [RelayCommand]
        public async Task ChangePushToTalkHotkey()
        {
            AwaitingHotkeyPress = true;
            ResetVolume();

            var newKeybindKeys = await keyRecorder.RecordKeysAsync();

            AwaitingHotkeyPress = false;

            if (newKeybindKeys == null)
                return;

            var newKeybind = new Keybind(keyboardListener, newKeybindKeys);
            UpdatePttKeybind(newKeybind);
        }
        #endregion

        private readonly float lastVolumeSetting;
        private readonly IKeyboardListener keyboardListener;
        private readonly SystemVolumeConfigurator volumeConfig;
        private readonly IKeyRecorder keyRecorder;

        public MainViewModel(IKeyboardListener keyboardListener, SystemVolumeConfigurator volumeConfig, IKeyRecorder keyRecorder)
        {
            this.volumeConfig = volumeConfig;
            this.lastVolumeSetting = this.volumeConfig.MasterVolumePercentage;
            this.keyboardListener = keyboardListener;
            this.keyboardListener.KeyDown += KeyboardListener_KeyDown;
            this.keyRecorder = keyRecorder;
        }

        private void KeyboardListener_KeyDown(object? sender, KeyboardEventArgs e)
        {
            if (AwaitingHotkeyPress && e.KeyCode == VKey.ESCAPE)
                keyRecorder.CancelRecording();
        }

        private void PttKeybind_KeybindReleased(object? sender, EventArgs e)
        {
            if (AwaitingHotkeyPress)
                return;

            ResetVolume();
        }

        private void PttKeybind_KeybindPressed(object? sender, EventArgs e)
        {
            if (AwaitingHotkeyPress)
                return;

            ReduceVolume();
        }

        private void ResetVolume()
        {
            volumeConfig.MasterVolumePercentage = lastVolumeSetting;
        }

        private void ReduceVolume()
        {
            volumeConfig.ReduceVolume(VolumeReductionPercentage);
        }

        private void UpdatePttKeybind(Keybind newKeybind)
        {
            if (PttKeybind != null)
            {
                PttKeybind.KeybindPressed -= PttKeybind_KeybindPressed;
                PttKeybind.KeybindReleased -= PttKeybind_KeybindReleased;
            }

            PttKeybind = newKeybind;
            PttKeybind.KeybindPressed += PttKeybind_KeybindPressed;
            PttKeybind.KeybindReleased += PttKeybind_KeybindReleased;
        }

        public void SetPushToTalkKeys(params VKey[] keys)
        {
            if (keys != null && keys.Length > 0)
                UpdatePttKeybind(new Keybind(this.keyboardListener, keys));
        }
    }
}
