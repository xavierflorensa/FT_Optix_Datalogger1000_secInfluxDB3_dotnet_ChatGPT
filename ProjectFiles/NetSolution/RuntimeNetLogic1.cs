#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.DataLogger;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Store;
using FTOptix.InfluxDBStoreRemote;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.InfluxDBStore;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using FTOptix.NetLogic;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;

#endregion

public class RuntimeNetLogic1 : BaseNetLogic
{
    PeriodicTask myTask1;
    
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        myTask1 = new PeriodicTask(write_Function, 1000, LogicObject);
        myTask1.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        myTask1.Dispose();
    }

    private void write_Function()
    {
        try
        {
            // Read the real-time value from the process variable
            var variable1 = Project.Current.GetVariable("Model/Variable1");
            int sencer = variable1.Value;

            // InfluxDB v3 configuration
            string url = "http://127.0.0.1:8181";
            string token = "apiv3_33VhRYYjZdWdZQdum74hpF3tpX0pDNwmqpY7yOL_TCPTRM-KnEwbpYLr612L-T-9PDSZ0RJZsB38-LU5LYMFBQ";
            string database = "OPTIX1";
            string table = "DataLogger1";

            // Create InfluxDB v3 client
            using (var client = new InfluxDBClient(host: url, token: token, database: database))
            {
                // Create write point for InfluxDB v3
                var point = PointData.Measurement(table)
                    .SetTag("host", "server01")
                    .SetField("value", sencer)
                    .SetTimestamp(DateTime.UtcNow);

                // Write data to InfluxDB v3 database
                client.WritePointAsync(point: point).Wait();
                Log.Info($"Value {sencer} inserted successfully into InfluxDB v3 database {database}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error inserting value into InfluxDB: {ex.Message}");
        }
    }

}
