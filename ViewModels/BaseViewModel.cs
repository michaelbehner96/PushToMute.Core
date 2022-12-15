using CommunityToolkit.Mvvm.ComponentModel;

namespace PushToMute.Core.ViewModels

{
    public partial class BaseViewModel : ObservableRecipient
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string? _statusMessage;

    }
}
