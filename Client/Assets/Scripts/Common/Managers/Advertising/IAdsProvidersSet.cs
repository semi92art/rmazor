using System.Collections.Generic;

namespace Common.Managers.Advertising
{
    public interface IAdsProvidersSet
    {
        List<IAdsProvider> GetProviders();
    }
}