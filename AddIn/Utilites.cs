using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
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

        public static FeatureLayer CheckAndCreateFeatureLayer(FeatureClass featureClass)
        {
            Uri featureClassUri = featureClass.GetPath();
            Map map = MapView.Active.Map;
            IEnumerable<FeatureLayer> layers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                                              .Where(lyr => lyr.GetFeatureClass().GetPath() == featureClassUri);
            if (layers.Any())
            {
                return layers.First();
            }
            return LayerFactory.Instance.CreateFeatureLayer(featureClass, map, layerName: featureClass.GetName());
        }

        public static Geodatabase ProjectDefaultGDB()
        {
            Uri uri = new Uri(Project.Current.DefaultGeodatabasePath);
            FileGeodatabaseConnectionPath connectionPath = new FileGeodatabaseConnectionPath(uri);
            Geodatabase geodatabase = new Geodatabase(connectionPath);
            return geodatabase;
        }
    }
}