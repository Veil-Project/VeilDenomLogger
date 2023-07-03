using FluentFTP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VeilBlockToDB.ModelsJson;
using VeilBlockToDB.Procs;
using System.Configuration;

namespace VeilBlockToDB
{
    public partial class frmMain : Form
    {
        private Timer _ApiCallTimer;

        private int _iTimerCounter = 1;
        private int _iLogCounter = 1;

        public frmMain()
        {
            InitializeComponent();
        }

        #region  "Form Buttons"
        private void BtnStart_Click(object sender, EventArgs e)
        {
            FillDenomEfficiency();
            InitTimer();
            UpdateAppStatus("Timer started...");
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            _ApiCallTimer.Stop();
            UpdateAppStatus("Timer stopped.");
        }     
        #endregion

        public void InitTimer()
        {
            if (_ApiCallTimer == null || !_ApiCallTimer.Enabled)
            {
                _ApiCallTimer = new Timer();
                _ApiCallTimer.Tick += new EventHandler(ApiCallTimer_Tick);
                _ApiCallTimer.Interval = 60000; // Every 60 seconds.
                _ApiCallTimer.Start();
            }
        }

        private void ApiCallTimer_Tick(object sender, EventArgs e)
        {
            _ApiCallTimer.Stop();
            FillDenomEfficiency();
                        
            if (_iTimerCounter % 30 == 0)  // upload to website every 30 minutes.
            {
                var colFilesToUpload = new List<DatasetUpload>();
                colFilesToUpload.AddRange(CreateEfficiencyJsonDatasets());
                UploadToSite(colFilesToUpload);
                colFilesToUpload.Clear();
                _iTimerCounter = 1;
            }

            _iTimerCounter++;
            _ApiCallTimer.Start();
        }

        public void FillDenomEfficiency()
        {
            UpdateAppStatus("FillDenomEfficiency Start...");
            var lOldestDenomEfficiencyBlock = GetDenomsToProcess();

            foreach (var item in lOldestDenomEfficiencyBlock)
            {
                UpdateAppStatus("InsertDenomEfficiency Block #: " + item);
                JsonDataset.InsertDenomEfficiency(item);
            }

            UpdateAppStatus("FillDenomEfficiency complete");
        }

        private List<long> GetDenomsToProcess()
        {
            //[]
            var colBlocks = new List<long>();
            var _dbVeilContext = new VeilContext();
            try
            {
                _dbVeilContext.Database.Connection.Open();

                var cmd = _dbVeilContext.Database.Connection.CreateCommand();
                cmd.CommandText = "sp_GetBlocksWithoutDenomEfficiency";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    colBlocks.Add(long.Parse(rdr["BlockID"].ToString()));
                }
            }
            catch (Exception ex)
            {
                var o = ex.Message;
                UpdateAppStatus("Delete block error: " + ex.Message);
            }
            finally
            {
                _dbVeilContext.Database.Connection.Close();
            }
            return colBlocks;
        }


        public static List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name], null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        private void UpdateAppStatus(string message)
        {
            UpdateAppStatus(message, true);
        }

        private void UpdateAppStatus(string message, bool appendMessage)
        {
            if (_iLogCounter == 200)
            {
                appendMessage = false;
                _iLogCounter = 0;
            }

            if (appendMessage)
            {
                rtxStatus.Text = string.Format("{0}: {1}{2}{3}", DateTime.Now.ToString("MM-dd-yy hh:mm:ss"), message,
                                                Environment.NewLine, rtxStatus.Text);
            }
            else
            {
                rtxStatus.Text = DateTime.Now.ToString("MM-dd-yy hh:mm:ss") + ":  " + message;
            }
            _iLogCounter++;
            Application.DoEvents();
        }


        #region Create Datasets

        private List<DatasetUpload> CreateEfficiencyJsonDatasets()
        {
            UpdateAppStatus("Starting to create Efficiency dataset...");
            var colFilesToUpload = new List<DatasetUpload>();
            colFilesToUpload.Add(ToDataUpload(new { Efficiency = JsonDataset.GetDenomEfficiencyDB(2, 1440) }, "DenomEfficiency", 1));
            colFilesToUpload.Add(ToDataUpload(new { Efficiency = JsonDataset.GetDenomEfficiencyDB(2, 4320) }, "DenomEfficiency", 2));
            colFilesToUpload.Add(ToDataUpload(new { Efficiency = JsonDataset.GetDenomEfficiencyDB(2, 10080) }, "DenomEfficiency", 3));

            UpdateAppStatus("Create Efficiency dataset complete");
            return colFilesToUpload;
        }

        #endregion

        #region DatasetUpload
        private void UploadToSite(List<DatasetUpload> jsonDatasets)
        {
            if (chkSaveToFileSystem.Checked)
            {
                try
                {
                    foreach (var szDataset in jsonDatasets)
                    {
                        SaveToFileSystem(szDataset);
                    }
                }
                catch (Exception ex)
                {
                    UpdateAppStatus("Save local file error: " + ex.Message);
                }
            }
            else
            {
                var ftpurl = ConfigurationManager.AppSettings["ftpUrl"];
                var ftpUsername = ConfigurationManager.AppSettings["ftpUsername"];
                var ftpPassword = ConfigurationManager.AppSettings["ftpPassword"];

                FtpClient client = new FtpClient(ftpurl);

                // if you don't specify login credentials, we use the "anonymous" user account
                client.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                try
                {
                    // begin connecting to the server
                    client.Connect();

                    foreach (var szDataset in jsonDatasets)
                    {
                        if (chkSaveToFileSystem.Checked)
                        {
                            SaveToFileSystem(szDataset);
                        }
                        else
                        {
                            UploadToSiteSub(szDataset, client);
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateAppStatus("Uploading file error: " + ex.Message);
                }
                finally
                {
                    client.Disconnect();
                }
            }
        }

        private DatasetUpload ToDataUpload(object data, string source, int index)
        {
            var oNewDatasetUpload = new DatasetUpload();
            oNewDatasetUpload.Dataset = CompressJson(JsonConvert.SerializeObject(data, Formatting.None,
                        new JsonSerializerSettings
                        {
                            DateFormatHandling = DateFormatHandling.IsoDateFormat
                        }));
            oNewDatasetUpload.Source = source;
            oNewDatasetUpload.Index = index;
            return oNewDatasetUpload;
        }

        private string CompressJson(string jsonData)
        {
            byte[] compressedBytes;
            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
            {
                using (var compressedStream = new MemoryStream())
                {
                    // setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
                    // this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
                    // although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
                    using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
                    {
                        uncompressedStream.CopyTo(compressorStream);
                    }

                    // call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
                    compressedBytes = compressedStream.ToArray();
                }
            }

            return Convert.ToBase64String(compressedBytes);
        }

        private void UploadToSiteSub(DatasetUpload jsonDataset, FtpClient client)
        {
            UpdateAppStatus("Uploading json dataset: " + jsonDataset.Source + " Index: " + jsonDataset.Index);
            var szUploadPath = "/httpdocs/JsonDatasets/" + jsonDataset.Source;
            if (jsonDataset.Index > 0)
            {
                szUploadPath += "/" + jsonDataset.Index;
            }
            szUploadPath += "/data.json";
            client.Upload(GenerateStreamFromString(jsonDataset.Dataset), szUploadPath, FtpExists.Overwrite, true);
            UpdateAppStatus("Uploading file complete: " + szUploadPath);
        }

        private void SaveToFileSystem(DatasetUpload jsonDataset)
        {
            UpdateAppStatus("Save json dataset: " + jsonDataset.Source + " Index: " + jsonDataset.Index);
            var szUploadPath = Path.Combine(txtSavePath.Text, "JsonDatasets", jsonDataset.Source);
            if (jsonDataset.Index > 0)
            {
                szUploadPath += @"\" + jsonDataset.Index;
            }
            szUploadPath += @"\data.json";
            File.WriteAllText(szUploadPath, jsonDataset.Dataset);
            UpdateAppStatus("Save file complete: " + szUploadPath);
        }
        #endregion



        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}

