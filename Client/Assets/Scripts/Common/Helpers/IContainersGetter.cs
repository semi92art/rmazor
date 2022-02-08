using UnityEngine;

namespace Common.Helpers
{
    public interface IContainersGetter
    {
        Transform GetContainer(string _Name);
    }
}