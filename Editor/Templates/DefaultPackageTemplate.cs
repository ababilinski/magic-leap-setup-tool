#region

using System.Collections.Generic;

#endregion

namespace MagicLeapSetupTool.Editor.Templates
{
    //TODO: Move to external file (.txt,.json.yaml,...)
    /// <summary>
    /// The template to use when updating the manifest file
    /// </summary>
    public static class DefaultPackageTemplate
    {
        public static readonly List<string> DEFAULT_PRIVILEGES = new List<string>()
        {
            "ControllerPose",
            "GesturesConfig",
            "GesturesSubscribe",
            "HandMesh",
            "ImuCapture",
            "Internet",
            "PcfRead",
            "WifiStatusRead",
            "WorldReconstruction",
            "AddressBookRead",
            "AddressBookWrite",
            "LocalAreaNetwork",
            "ObjectData",
            "AudioCaptureMic",
            "CameraCapture",
            "ComputerVision",
            "FineLocation"
        };
    }
}