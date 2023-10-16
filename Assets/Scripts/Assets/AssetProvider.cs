using UnityEngine;

namespace Assets
{
    public class AssetProvider : IAssetProvider {
        public GameObject Instantiate(string path) =>
                    Object.Instantiate(Resources.Load<GameObject>(path));
        public GameObject InstantiateWithParent(string path, Transform parent) =>
			        Object.Instantiate(Resources.Load<GameObject>(path), parent.transform, false);
    }
}