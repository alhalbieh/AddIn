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
using ArcGIS.Desktop.Core.Geoprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using MessageBoxWPF = System.Windows.MessageBox;

namespace AddIn
{
    internal class MapTool1 : MapTool
    {
        public MapTool1()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Polygon;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            MessageBoxResult result = MessageBox.Show(messageText: "Confirm shape?", caption: "Confirm Shape",
                                                      button: MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                MessageBox.Show(geometry.HasZ.ToString());
                return true;
            }
            await QueuedTask.Run(() =>
            {
                try
                {
                    Geodatabase geodatabase = Utilites.ProjectDefaultGDB();

                    FeatureClass towerRangeFC = geodatabase.OpenDataset<FeatureClass>("TowerRange");
                    RowCursor towerRangeCursor = towerRangeFC.Search(null, false);
                    List<Geometry> ranges = new List<Geometry>();
                    if (towerRangeFC.GetCount() == 0)
                    {
                        MessageBox.Show("Tower ranges are not calculated");
                        return;
                    }
                    while (towerRangeCursor.MoveNext())
                    {
                        Feature range = towerRangeCursor.Current as Feature;
                        ranges.Add(range.GetShape());
                    }
                    Geometry rangesUnion = GeometryEngine.Instance.Union(ranges);
                    /*
                    FeatureClass serviceAreaFC = geodatabase.OpenDataset<FeatureClass>("ServiceArea");
                    RowCursor serviceAreaCursor = serviceAreaFC.Search(null, false);
                    EditOperation editOperation1 = new EditOperation();
                    editOperation1.Callback(context =>
                    {
                        try
                        {
                            if (serviceAreaCursor.MoveNext())
                            {
                                Feature serviceAreaFeature = serviceAreaCursor.Current as Feature;
                                serviceAreaFeature.SetShape(geometry);
                                serviceAreaFeature.Store();
                            }
                            else
                            {
                                RowBuffer serviceAreaBuffer = serviceAreaFC.CreateRowBuffer();
                                Feature serviceAreaFeature = serviceAreaFC.CreateRow(serviceAreaBuffer);
                                serviceAreaFeature.SetShape(geometry);
                            }
                        }
                        catch (Exception error)
                        {
                            MessageBox.Show(error.ToString());
                        }
                    }, serviceAreaFC);
                    editOperation1.ExecuteAsync();
                    */
                    //Geoprocessing.ExecuteToolAsync(toolb)
                    /*
                    if (serviceAreaFC.GetCount() == 0)
                    {
                        RowBuffer serviceAreaBuffer = serviceAreaFC.CreateRowBuffer();
                        Row serviceAreaTemp = serviceAreaFC.CreateRow(serviceAreaBuffer);
                        serviceAreaTemp.Store();
                    }

                    serviceAreaCursor.MoveNext();
                    Feature serviceArea = (Feature)serviceAreaCursor.Current;
                    serviceArea.SetShape(geometry);
                    serviceArea.Store();
                    */
                    //Geometry serviceAreaGeometry = null;

                    //double deadAreaPercentage = 0;
                    //if (serviceAreaCursor.MoveNext())
                    //{
                    //Row serviceAreaRow = serviceAreaCursor.Current;
                    //serviceAreaGeometry = (serviceAreaRow as Feature).GetShape();

                    //deadAreaGeometry = GeometryEngine.Instance.Difference(geometry, rangesUnion);
                    //deadAreaPercentage = GeometryEngine.Instance.Area(deadAreaGeometry) / GeometryEngine.Instance.Area(serviceAreaGeometry);
                    //deadAreaPercentage = GeometryEngine.Instance.Area(deadAreaGeometry) / GeometryEngine.Instance.Area(geometry);

                    //serviceAreaRow["DEADCOVERAGE"] = deadAreaPercentage * 100;
                    //serviceAreaRow["RECEPTIONCOVERAGE"] = 100 * (1 - deadAreaPercentage);
                    //serviceAreaRow.Store();
                    //}
                    //else
                    //{
                    //  MessageBox.Show("Add a feature in the \"ServiceArea\" feature class");
                    //return;
                    //}
                    /*
                    serviceAreaCursor = serviceAreaFC.Search(null, false);
                    serviceAreaCursor.MoveNext();
                    Feature serviceArea = serviceAreaCursor.Current as Feature;
                    */
                    Geometry deadAreaGeometry = GeometryEngine.Instance.Intersection(rangesUnion, geometry);
                    FeatureClass deadAreaFC = geodatabase.OpenDataset<FeatureClass>("DeadArea");
                    RowCursor deadAreaCursor = deadAreaFC.Search(null, false);
                    EditOperation editOperation2 = new EditOperation();
                    editOperation2.Callback(context =>
                    {
                        try
                        {
                            if (deadAreaCursor.MoveNext())
                            {
                                Feature deadAreaFeature = deadAreaCursor.Current as Feature;
                                deadAreaFeature.SetShape(deadAreaGeometry);
                                deadAreaFeature.Store();
                            }
                            else
                            {
                                RowBuffer deadAreaBuffer = deadAreaFC.CreateRowBuffer();
                                Feature deadAreaFeature = deadAreaFC.CreateRow(deadAreaBuffer);
                                deadAreaFeature.SetShape(deadAreaGeometry);
                            }
                        }
                        catch (Exception error)
                        {
                            MessageBox.Show(error.ToString());
                        }
                    }, deadAreaFC);
                    editOperation2.ExecuteAsync();

                    FeatureLayer layer = Utilites.CheckAndCreateFeatureLayer(deadAreaFC);
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString());
                }
            });
            return true;
        }
    }
}