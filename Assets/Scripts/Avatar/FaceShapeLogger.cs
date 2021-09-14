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
        [SerializeField] private string logFolder = "C:/Logging";
        [Range(0.05f, 0.25f)] [SerializeField] private float loggingRate = 0.1f; // # of logs per second 
        
        private Dictionary<LipShape_v2, float> _lipWeightings;
        private Dictionary<EyeShape_v2, float> _eyeWeightings;
        private static EyeData_v2 _eyeData;

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
            _logFile = logFolder + $"\\subj_{_subjectID}.txt";
            
            // Create file or clear file of potential contents
            using(FileStream fs = File.Open(_logFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                lock(fs)
                {
                    fs.SetLength(0);
                }
            }
            
            // Init streamwriter
            _sw = File.AppendText(_logFile);
            
            InvokeRepeating(nameof(LogTick), 0,loggingRate);
        }

        private void OnApplicationQuit()
        {
            if (activateLogging) _sw.Close();
        }

        public void LogTick()
        {
            if (!activateLogging) return;
            SRanipal_Eye_API.GetEyeData_v2(ref _eyeData);  // Mal schauen was passiert
            SRanipal_Eye_v2.GetEyeWeightings(out _eyeWeightings, _eyeData);
            SRanipal_Lip_v2.GetLipWeightings(out _lipWeightings);

            if (!_header) CreateHeader(_eyeWeightings, _lipWeightings);

            LogFaceShapes(_eyeWeightings, _lipWeightings);
        }

        private void LogFaceShapes(Dictionary<EyeShape_v2, float> eyeShapeWeightings, Dictionary<LipShape_v2, float> lipShapeWeightings)
        {
            if (eyeShapeWeightings == null || lipShapeWeightings == null) return;

            var logstring = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            foreach (KeyValuePair<EyeShape_v2, float> item in eyeShapeWeightings)
            {
                logstring += string.Format(CultureInfo.InvariantCulture, "\t{0}", item.Value);
                
            }
            foreach (KeyValuePair<LipShape_v2, float> item in lipShapeWeightings)
            {
                logstring += string.Format(CultureInfo.InvariantCulture, "\t{0}", item.Value);
            }
        
            _sw.WriteLine(logstring);
        }

        private void CreateHeader(Dictionary<EyeShape_v2, float> eyeShapeWeightings,
            Dictionary<LipShape_v2, float> lipShapeWeightings)
        {
            if (eyeShapeWeightings == null || lipShapeWeightings == null) return;

            var logstring = "Time";
            foreach (KeyValuePair<EyeShape_v2, float> item in eyeShapeWeightings)
            {
                if (item.Key < EyeShape_v2.Max || item.Key > 0)
                {
                    logstring += $"\t{item.Key}";
                }
            }
            foreach (KeyValuePair<LipShape_v2, float> item in lipShapeWeightings)
            {
                if (item.Key < LipShape_v2.Max || item.Key > 0)
                {
                    logstring += $"\t{item.Key}";
                }
            }
        
            _sw.WriteLine(logstring);

            _header = true;
        }
    }
}
            


