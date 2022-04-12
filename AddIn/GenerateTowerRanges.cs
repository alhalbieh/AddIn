﻿using ArcGIS.Core.CIM;
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
    internal class GenerateTowerRanges : Button
    {
        protected override async void OnClick()
        {
            await QueuedTask.Run(() =>
            {
                try
                {
                    Uri uri = new Uri(Project.Current.DefaultGeodatabasePath);
                    FileGeodatabaseConnectionPath connectionPath = new FileGeodatabaseConnectionPath(uri);
                    Geodatabase geodatabase = new Geodatabase(connectionPath);

                    FeatureClass towerFC = geodatabase.OpenDataset<FeatureClass>("Tower");
                    Table towerDetailsTable = geodatabase.OpenDataset<Table>("TowerDetails");
                    RowCursor towerCursor = towerFC.Search(null, false);

                    FeatureClass towerRangeFC = geodatabase.OpenDataset<FeatureClass>("TowerRange");
                    EditOperation editOperation = new EditOperation();
                    editOperation.ShowProgressor = true;
                    editOperation.Callback(context =>
                    {
                        try
                        {
                            towerRangeFC.DeleteRows(new QueryFilter());
                            while (towerCursor.MoveNext())
                            {
                                double range = 0;
                                Row tower = towerCursor.Current;
                                string towerType = (string)tower["TOWERTYPE"];
                                QueryFilter queryFilter = new QueryFilter { WhereClause = $"TOWERTYPE = '{towerType}'" };
                                RowCursor queryCursor = towerDetailsTable.Search(queryFilter);
                                if (queryCursor.MoveNext())
                                {
                                    range = (double)queryCursor.Current["TOWERCOVERAGE"];
                                }

                                Geometry towerShape = ((Feature)tower).GetShape();
                                Geometry smallRange = GeometryEngine.Instance.Buffer(towerShape, range / 3);
                                Geometry mediumRange = GeometryEngine.Instance.Buffer(towerShape, 2 * range / 3);
                                Geometry bigRange = GeometryEngine.Instance.Buffer(towerShape, range);

                                Geometry range3Bars = smallRange;
                                Geometry range2Bars = GeometryEngine.Instance.SymmetricDifference(smallRange, mediumRange);
                                Geometry range1Bars = GeometryEngine.Instance.SymmetricDifference(mediumRange, bigRange);

                                List<Geometry> ranges = new List<Geometry> { range3Bars, range2Bars, range1Bars };
                                for (int i = 0; i < 3; i++)
                                {
                                    RowBuffer rowBuffer = towerRangeFC.CreateRowBuffer();
                                    rowBuffer["BARS"] = 3 - i;
                                    rowBuffer["TOWERID"] = tower["TOWERID"];
                                    Feature newFeature = towerRangeFC.CreateRow(rowBuffer);
                                    newFeature.SetShape(ranges[i]);
                                }
                            }
                        }
                        catch (Exception error)
                        {
                            MessageBox.Show(error.ToString());
                        }
                    }, towerRangeFC);
                    editOperation.ExecuteAsync();

                    FeatureLayer layer = Utilites.CheckAndCreateFeatureLayer(towerRangeFC);
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString());
                }
            });
        }
    }
}