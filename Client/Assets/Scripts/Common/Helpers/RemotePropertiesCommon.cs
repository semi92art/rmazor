namespace Common.Helpers
{
    public interface IRemotePropertiesCommon
    {
        bool DebugEnabled { get; set; }
    }
    
    public class RemotePropertiesCommon : IRemotePropertiesCommon
    {
        public bool DebugEnabled  { get; set; }
        string      TestDeviceIds { get; set; }
    }
}