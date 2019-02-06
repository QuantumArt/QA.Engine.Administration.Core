namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface ISettingsProvider
    {
        int GetContentId(int siteId);
        bool HasRegion(int siteId);
    }
}
