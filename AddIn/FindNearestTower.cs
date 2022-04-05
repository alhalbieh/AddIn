using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddIn
{
    internal class FindNearestTower : MapTool
    {
        public FindNearestTower()
        {
        }

        protected override void OnToolMouseUp(MapViewMouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override async Task HandleMouseUpAsync(MapViewMouseButtonEventArgs e)
        {
            await QueuedTask.Run(() =>
            {
                try
                {
                    Uri uri = new Uri(Project.Current.DefaultGeodatabasePath);
                    FileGeodatabaseConnectionPath connectionPath = new FileGeodatabaseConnectionPath(uri);
                    Geodatabase geodatabase = new Geodatabase(connectionPath);

                    MapPoint mapPoint = MapView.Active.ClientToMap(e.ClientPoint);
                    FeatureClass towerFC = geodatabase.OpenDataset<FeatureClass>("Tower");
                    RowCursor towerCursor = towerFC.Search();

                    double minDistance = double.PositiveInfinity;
                    string id = "";
                    while (towerCursor.MoveNext())
                    {
                        Feature tower = (Feature)towerCursor.Current;
                        double distance = GeometryEngine.Instance.Distance(tower.GetShape(), mapPoint);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            id = (string)tower["TOWERID"];
                        }
                    }

                    MessageBox.Show($"Nearest tower is {id}");
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString());
                }
            });
        }
    }
}