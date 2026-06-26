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
    private string influxUrl;
    private string influxToken;
    private string influxDatabase;
    private string influxTable;
    private InfluxDBClient influxClient;
    private IUAVariable variable1;
    
    
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        //var variable1 = Project.Current.GetVariable("Model/Variable1");
        variable1 = LogicObject.GetVariable("VariablePLC");
        // InfluxDB v3 configuration (initialize once)
        influxUrl = "http://127.0.0.1:8181";
        influxToken = "apiv3_33VhRYYjZdWdZQdum74hpF3tpX0pDNwmqpY7yOL_TCPTRM-KnEwbpYLr612L-T-9PDSZ0RJZsB38-LU5LYMFBQ";
        influxDatabase = "OPTIX1";
        influxTable = "DataLogger1";
        // Create InfluxDB client once and reuse it for every write
        influxClient = new InfluxDBClient(host: influxUrl, token: influxToken, database: influxDatabase);

        myTask1 = new PeriodicTask(write_Function, 100, LogicObject);
        myTask1.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        myTask1.Dispose();
        influxClient?.Dispose();
    }

    private void write_Function()
    {
        try
        {
            // Read the real-time value from the process variable
            //var variable1 = Project.Current.GetVariable("Model/Variable1");
            
            int sencer = variable1.Value;

            // Reuse the InfluxDB client without recreating it each cycle
            var point = PointData.Measurement(influxTable)
                .SetTag("host", "server01")
                .SetField("value", sencer)
                .SetTimestamp(DateTime.UtcNow);

            influxClient.WritePointAsync(point: point).Wait();

            
        }
        catch (Exception ex)
        {
            Log.Error($"Error inserting value into InfluxDB: {ex.Message}");
        }
    }

}
