using Services;
using UnityEngine;

namespace Assets
{
    public interface IAssetProvider : IService {
        GameObject Instantiate(string path);
        GameObject InstantiateWithParent(string path, Transform parent);
    }
}