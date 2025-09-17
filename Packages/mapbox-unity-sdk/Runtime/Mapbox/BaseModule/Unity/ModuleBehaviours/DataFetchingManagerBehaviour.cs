using System;
using Mapbox.BaseModule.Data.DataFetchers;
using UnityEngine;

namespace Mapbox.Example.Scripts.ModuleBehaviours
{
    public class DataFetchingManagerBehaviour : MonoBehaviour
    {
        public DataFetchingManager DataFetcher;

        public DataFetchingManager GetDataFetchingManager() => DataFetcher;
        public DataFetchingManager GetDataFetchingManager(string accessToken, Func<string> skuTokenFunc)
        {
            if (DataFetcher == null)
            {
                DataFetcher = new DataFetchingManager(accessToken, skuTokenFunc);
            }

            return DataFetcher;
        }
    }
}
