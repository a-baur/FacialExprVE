//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
//========= Modified for networking with Photon
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Newtonsoft.Json;

namespace ViveSR.anipal.Lip
{
    public class NetworkSRanipalLip_v1 : MonoBehaviour
    {
        [SerializeField] private List<LipShapeTable> LipShapeTables;

        public bool NeededToGetData = true;
        private Dictionary<LipShape, float> LipWeightings;

        private PhotonView _photonView;


        private void Start()
        {
            _photonView = GetComponent<PhotonView>();
            if (!SRanipal_Lip_Framework.Instance.EnableLip)
            {
                Debug.LogError("[SRanipal] Lip disabled!");
                enabled = false;
                return;
            }

            SetLipShapeTables(LipShapeTables);
        }

        private void Update()
        {
            if (!SRanipal_Lip_Framework.Instance.EnableLip)
            {
                enabled = false;
                return;
            }

            if (NeededToGetData && _photonView.IsMine)
            {
                SRanipal_Lip.GetLipWeightings(out LipWeightings);
                Dictionary<byte, float> binLipWeightings = BinaryLipWeightings(LipWeightings);
                        
                // Update Blendshapes for all players
                _photonView.RPC("UpdateLipShapes", RpcTarget.All, binLipWeightings);
                        
            }
        }
        
        // Initilize Blendshape-Lipshape mapping
        public void SetLipShapeTables(List<LipShapeTable> lipShapeTables)
        {
            bool valid = true;
            if (lipShapeTables == null)
            {
                valid = false;
                Debug.LogError("[SRanipal] No blendshape mappings defined. Must define Lip Shape Table.");
            }
            else
            {
                for (int table = 0; table < lipShapeTables.Count; ++table)
                {
                    // Check that LipShapeTable has corresponding SkinnedMeshRenderer
                    SkinnedMeshRenderer tableSMR = lipShapeTables[table].skinnedMeshRenderer;
                    if (tableSMR == null)
                    {
                        valid = false;
                        Debug.LogError($"[SRanipal] SkinnedMeshRenderer in Lip Shape Table {table} not found.");
                        break;
                    }
                    for (int shape = 0; shape < lipShapeTables[table].lipShapes.Length; ++shape)
                    {
                        // Check that LipShape is within range of possible LipShapes
                        // Original: Fail when Blendshape is assigned unvalid LipShape (e.g. None)
                        // Modified: Ignore unvalid Blendshapes. Not every Blendshape needs to be assigned to LipShape.
                        LipShape lipShape = lipShapeTables[table].lipShapes[shape];
                        if (lipShape > LipShape.Max || lipShape < 0)
                        {
                            string blendshape = tableSMR.sharedMesh.GetBlendShapeName(shape);
                            Debug.LogWarning($"[SRanipal] Blendshape '{blendshape}' assigned to invalid LipShape '{lipShape}'. Facial animation may not be accurate!");
                            continue;
                        }
                    }
                }
            }
            if (valid)
            {
                LipShapeTables = lipShapeTables;
            }
        }

        // Update all blendshapes models in LipShapeTables.
        // Call on remote.
        [PunRPC]
        public void UpdateLipShapes(Dictionary<byte, float> binLipWeightings)
        {
            Dictionary<LipShape, float> lipWeightings = EnumLipWeightings(binLipWeightings);
                    
            foreach (var table in LipShapeTables)
                RenderModelLipShape(table, lipWeightings);
        }

        // Apply Facial Tracker weightings onto blendshape model
        private void RenderModelLipShape(LipShapeTable lipShapeTable, Dictionary<LipShape, float> weighting)
        {
            for (int i = 0; i < lipShapeTable.lipShapes.Length; i++)
            {
                int targetIndex = (int)lipShapeTable.lipShapes[i];
                if (targetIndex > (int)LipShape.Max || targetIndex < 0) continue;
                var lipShape = (LipShape)targetIndex;
                if (!weighting.ContainsKey(lipShape))
                {
                    Debug.Log("[SRanipal EYE NETWORKED]LipShape " + lipShape + " not in weightings");
                    continue;
                }
                lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[lipShape] * 100);
            }
        }
                
        /// Converts LipWeightings from enumerated to binary.
        /// Enables serialization for Photon.
        private Dictionary<byte, float> BinaryLipWeightings(Dictionary<LipShape, float> lipWeightings)
        {
            Dictionary<byte, float> binLipWeightings = new Dictionary<byte, float>();
                    
            foreach (KeyValuePair<LipShape, float> lipWeight in lipWeightings)
            {
                binLipWeightings[(byte) lipWeight.Key] = lipWeight.Value;
            }

            return binLipWeightings;
        }
                
        // Converts LipWeightings from binary to enumerated.
        private Dictionary<LipShape, float> EnumLipWeightings(Dictionary<byte, float> binLipWeightings)
        {
            Dictionary<LipShape, float> lipWeightings = new Dictionary<LipShape, float>();
                    
            foreach (KeyValuePair<byte, float> lipWeight in binLipWeightings)
            {
                lipWeightings[(LipShape) lipWeight.Key] = lipWeight.Value;
            }

            return lipWeightings;
        }
    }
}