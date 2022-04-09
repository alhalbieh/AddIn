using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddIn
{
    internal class Utilites
    {
        public static void ReplaceFeatureLayer(FeatureClass featureClass)
        {
            string editedFCName = featureClass.GetName();
            Map map = MapView.Active.Map;
            IEnumerable<FeatureLayer> layers = map.FindLayers(editedFCName).OfType<FeatureLayer>();

            foreach (FeatureLayer layer in layers)
            {
                string layerName = layer.GetFeatureClass().GetName();
                if (layerName == editedFCName)
                {
                    map.RemoveLayer(layer);
                }
            }
            LayerFactory.Instance.CreateFeatureLayer(featureClass, map, layerName: editedFCName);
        }

        public static void CheckAndCreateFeatureLayer(FeatureClass featureClass)
        {
            string editedFCName = featureClass.GetName();
            Map map = MapView.Active.Map;
            IEnumerable<FeatureLayer> layers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>();

            foreach (FeatureLayer layer in layers)
            {
                string layerName = layer.GetFeatureClass().GetName();
                if (layerName == editedFCName)
                {
                    return;
                }
            }
            LayerFactory.Instance.CreateFeatureLayer(featureClass, map, layerName: editedFCName);
        }
    }
}