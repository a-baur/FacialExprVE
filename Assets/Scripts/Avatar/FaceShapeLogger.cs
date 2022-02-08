using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;

namespace Avatar
{
    public class FaceShapeLogger : MonoBehaviour
    {
        [SerializeField] private bool activateLogging;
        [Range(0.05f, 0.25f)] [SerializeField] private float loggingRate = 0.1f; // # of logs per second 
        
        private Dictionary<LipShape, float> _lipWeightings;
        private Dictionary<EyeShape, float> _eyeWeightings;
        private static EyeData _eyeData;

        private string _logFolder;
        private string _subjectID;
        private string _logFile;
        private StreamWriter _sw;

        // Use to check if header row has been created
        private bool _header = false;

        private bool active;

        public void StartLogging()
        {
            if (!activateLogging | !SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }
            
            _subjectID = FindObjectOfType<GameSettings>().subjectID;
            // _logFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Logging\\FaceShapeLogs";  // to documents folder
            _logFolder = "Z:\\Bachelor\\Data\\Logging\\FaceShapeLogs"; // to network drive
            _logFile = _logFolder + $"\\subj_{_subjectID}.tsv";

            Debug.Log("[FaceShapeLogger] Starting logging in " + _logFile);

            // Create file or clear file of potential contents
            using (FileStream fs = File.Open(_logFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                lock(fs)
                {
                    fs.SetLength(0);
                }
            }
            
            // Init streamwriter
            _sw = File.AppendText(_logFile);
            
            InvokeRepeating(nameof(LogTick), 0, loggingRate);
        }

        private void OnApplicationQuit()
        {
            if (activateLogging && _sw != null) _sw.Close();
        }

        public void LogTick()
        {
            if (!activateLogging) return;
            SRanipal_Eye_API.GetEyeData(ref _eyeData);  // Mal gucken was passiert
            SRanipal_Eye.GetEyeWeightings(out _eyeWeightings, _eyeData);
            SRanipal_Lip.GetLipWeightings(out _lipWeightings);

            if (!_header) CreateHeader(_eyeWeightings, _lipWeightings);

            LogFaceShapes(_eyeWeightings, _lipWeightings);
        }

        // Create row from dictionary value. 
        private void LogFaceShapes(Dictionary<EyeShape, float> eyeShapeWeightings, Dictionary<LipShape, float> lipShapeWeightings)
        {
            if (eyeShapeWeightings == null || lipShapeWeightings == null) return;

            // Timestamp
            var logstring = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();

            // Eye shape values with ten dicimal places
            foreach (KeyValuePair<EyeShape, float> item in eyeShapeWeightings)
            {
                logstring += string.Format(CultureInfo.InvariantCulture, "\t{0:0.0000000000}", item.Value);
            }

            // Lip shape values with ten dicimal places
            foreach (KeyValuePair<LipShape, float> item in lipShapeWeightings)
            {
                logstring += string.Format(CultureInfo.InvariantCulture, "\t{0:0.0000000000}", item.Value);
            }

            // Write values
            _sw.WriteLine(logstring);
        }

        // Create header from dictionary keys. 
        private void CreateHeader(Dictionary<EyeShape, float> eyeShapeWeightings, Dictionary<LipShape, float> lipShapeWeightings)
        {
            if (eyeShapeWeightings == null || lipShapeWeightings == null) return;

            var logstring = "Time";
            foreach (KeyValuePair<EyeShape, float> item in eyeShapeWeightings)
            {
                if (item.Key < EyeShape.Max || item.Key > 0)
                {
                    logstring += $"\t{item.Key}";
                }
            }
            foreach (KeyValuePair<LipShape, float> item in lipShapeWeightings)
            {
                if (item.Key < LipShape.Max || item.Key > 0)
                {
                    logstring += $"\t{item.Key}";
                }
            }
        
            _sw.WriteLine(logstring);

            _header = true;
        }
    }
}
            


