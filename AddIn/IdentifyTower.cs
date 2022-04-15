using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Threading.Tasks;

namespace AddIn
{
    internal class IdentifyTower : MapTool
    {
        public IdentifyTower()
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
                    Geodatabase geodatabase = Utilites.ProjectDefaultGDB();

                    MapPoint mapPoint = MapView.Active.ClientToMap(e.ClientPoint);
                    Geometry buffer = GeometryEngine.Instance.Buffer(mapPoint, 4000);

                    SpatialQueryFilter spatialQuery = new SpatialQueryFilter
                    {
                        FilterGeometry = buffer,
                        SpatialRelationship = SpatialRelationship.Intersects
                    };

                    FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>("Tower");
                    RowCursor cursor = featureClass.Search(spatialQuery, true);
                    if (cursor.MoveNext())
                    {
                        Row row = cursor.Current;
                        MessageBox.Show($"Tower id: {row["TOWERID"]}\n Tower type: {row["TOWERTYPE"]}");
                    }
                    else
                    {
                        MessageBox.Show("Move closer to the tower");
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString());
                }
            });
        }
    }
}