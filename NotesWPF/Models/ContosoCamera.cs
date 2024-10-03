namespace ContosoDeviceManager
{
    public class ContosoCamera
    {
        public string? Title { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? Icon { get; set; }

        public List<string>? Images { get; set; }

        public bool FirmwareUpdateAvailable { get; set; } = false;
    }
}