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
    internal class GenerateDevice : MapTool
    {
        public GenerateDevice()
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
                Uri uri = new Uri(Project.Current.DefaultGeodatabasePath);
                FileGeodatabaseConnectionPath connectionPath = new FileGeodatabaseConnectionPath(uri);
                Geodatabase geodatabase = new Geodatabase(connectionPath);

                MapPoint mapPoint = MapView.Active.ClientToMap(e.ClientPoint);
                SpatialQueryFilter spatialQuery = new SpatialQueryFilter
                {
                    FilterGeometry = mapPoint,
                    SpatialRelationship = SpatialRelationship.Intersects
                };

                FeatureClass towerRangeFC = geodatabase.OpenDataset<FeatureClass>("TowerRange");
                if (towerRangeFC.GetCount() == 0)
                {
                    MessageBox.Show("Tower ranges is not calculated");
                    return;
                }
                RowCursor towerRangeCursor = towerRangeFC.Search(spatialQuery, false);

                FeatureClass deviceFC = geodatabase.OpenDataset<FeatureClass>("Device");

                EditOperation editOperation = new EditOperation();
                editOperation.Callback(context =>
                {
                    try
                    {
                        short maxBars = 0;
                        string towerID = "no tower";
                        while (towerRangeCursor.MoveNext())
                        {
                            Row towerRange = towerRangeCursor.Current;
                            short bars = (short)towerRange["BARS"];
                            string tid = (string)towerRange["TOWERID"];
                            if (bars > maxBars)
                            {
                                maxBars = bars;
                                towerID = tid;
                            }
                        }

                        if (deviceFC.GetCount() == 0)
                        {
                            RowBuffer deviceBuffer = deviceFC.CreateRowBuffer();
                            Row deviceTemp = deviceFC.CreateRow(deviceBuffer);
                            deviceTemp["DEVICEID"] = "D01";
                            deviceTemp.Store();
                        }
                        RowCursor deviceCursor = deviceFC.Search();
                        deviceCursor.MoveNext();
                        Feature device = (Feature)deviceCursor.Current;
                        device["BARS"] = maxBars;
                        device["CONNECTEDTOWERID"] = towerID;
                        device.SetShape(mapPoint);
                        device.Store();
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.ToString());
                    }
                }, deviceFC);
                editOperation.ExecuteAsync();
            });
        }
    }
}