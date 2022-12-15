using CSCore.CoreAudioAPI;

namespace PushToMute.Core.Services
{
    public class SystemVolumeConfigurator
    {
        public float MasterVolume
        {
            get => AudioEndpointVolume.FromDevice(playbackDevice).MasterVolumeLevelScalar;
            set => AudioEndpointVolume.FromDevice(playbackDevice).MasterVolumeLevelScalar = value;
        }

        public float MasterVolumePercentage
        {
            get => AudioEndpointVolume.FromDevice(playbackDevice).MasterVolumeLevelScalar * 100.0f;
            set => AudioEndpointVolume.FromDevice(playbackDevice).MasterVolumeLevelScalar = value / 100.0f;
        }

        private readonly MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
        private readonly MMDevice playbackDevice;

        public SystemVolumeConfigurator()
        {
            playbackDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        public void ReduceVolume(float percentage)
            => MasterVolumePercentage = MasterVolumePercentage - MasterVolume * percentage;
    }

}
