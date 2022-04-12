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
    internal class GenerateDeadAreas : Button
    {
        protected override async void OnClick()
        {
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

                    FeatureClass serviceAreaFC = geodatabase.OpenDataset<FeatureClass>("ServiceArea");
                    RowCursor serviceAreaCursor = serviceAreaFC.Search(null, false);
                    Geometry serviceAreaGeometry = null;
                    Geometry deadAreaGeometry = null;
                    double deadAreaPercentage = 0;
                    if (serviceAreaCursor.MoveNext())
                    {
                        Row serviceAreaRow = serviceAreaCursor.Current;
                        serviceAreaGeometry = (serviceAreaRow as Feature).GetShape();
                        deadAreaGeometry = GeometryEngine.Instance.SymmetricDifference(rangesUnion, serviceAreaGeometry);
                        deadAreaPercentage = GeometryEngine.Instance.Area(deadAreaGeometry) / GeometryEngine.Instance.Area(serviceAreaGeometry);

                        serviceAreaRow["DEADCOVERAGE"] = deadAreaPercentage * 100;
                        serviceAreaRow["RECEPTIONCOVERAGE"] = 100 * (1 - deadAreaPercentage);
                        serviceAreaRow.Store();
                    }
                    else
                    {
                        MessageBox.Show("Add a feature in the \"ServiceArea\" feature class");
                        return;
                    }

                    FeatureClass deadAreaFC = geodatabase.OpenDataset<FeatureClass>("DeadArea");
                    RowCursor deadAreaCursor = deadAreaFC.Search(null, false);

                    EditOperation editOperation = new EditOperation();
                    editOperation.Callback(context =>
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
                    editOperation.ExecuteAsync();

                    Utilites.CheckAndCreateFeatureLayer(deadAreaFC);
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString());
                }
            });
        }
    }
}