//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
//========= Modified for networking with Photon
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace ViveSR.anipal.Lip
{
    public class NetworkSRanipalLip : MonoBehaviour
    {
        [SerializeField] private List<LipShapeTable_v2> LipShapeTables;

        public bool NeededToGetData = true;
        private Dictionary<LipShape_v2, float> LipWeightings;

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
                SRanipal_Lip_v2.GetLipWeightings(out LipWeightings);
                Dictionary<byte, float> binLipWeightings = BinaryLipWeightings(LipWeightings);
                        
                // Update Blendshapes for all players
                _photonView.RPC("UpdateLipShapes", RpcTarget.All, binLipWeightings);
                        
            }
        }
        
        // Initilize Blendshape-Lipshape mapping
        public void SetLipShapeTables(List<LipShapeTable_v2> lipShapeTables)
        {
            bool valid = true;
            if (lipShapeTables == null)
            {
                valid = false;
            }
            else
            {
                for (int table = 0; table < lipShapeTables.Count; ++table)
                {
                    // Check that every LipShape has corresponding blendshape
                    if (lipShapeTables[table].skinnedMeshRenderer == null)
                    {
                        valid = false;
                        break;
                    }
                    for (int shape = 0; shape < lipShapeTables[table].lipShapes.Length; ++shape)
                    {
                        // Check that LipShape is within range of possible LipShapes
                        // Can only really fail, if version 1 and 2 are mixed up?
                        LipShape_v2 lipShape = lipShapeTables[table].lipShapes[shape];
                        if (lipShape > LipShape_v2.Max || lipShape < 0)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
            }
            if (valid)
            {
                LipShapeTables = lipShapeTables;
            }
            else
            {
                Debug.LogError("[SRanipal] Missing blendshapes for lip tracking. Facial animation unavailable!");
            }
        }

        // Update all blendshapes models with LipShapeTable.
        // Call on remote.
        [PunRPC]
        public void UpdateLipShapes(Dictionary<byte, float> binLipWeightings)
        {
            Dictionary<LipShape_v2, float> lipWeightings = EnumLipWeightings(binLipWeightings);
                    
            foreach (var table in LipShapeTables)
                RenderModelLipShape(table, lipWeightings);
        }

        // Apply weightings onto blendshape model
        private void RenderModelLipShape(LipShapeTable_v2 lipShapeTable, Dictionary<LipShape_v2, float> weighting)
        {
            for (int i = 0; i < lipShapeTable.lipShapes.Length; i++)
            {
                int targetIndex = (int)lipShapeTable.lipShapes[i];
                if (targetIndex > (int)LipShape_v2.Max || targetIndex < 0) continue;
                lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[(LipShape_v2)targetIndex] * 100);
            }
        }
                
        /// Converts LipWeightings from enumerated to binary.
        /// Enables serialization for Photon.
        private Dictionary<byte, float> BinaryLipWeightings(Dictionary<LipShape_v2, float> lipWeightings)
        {
            Dictionary<byte, float> binLipWeightings = new Dictionary<byte, float>();
                    
            foreach (KeyValuePair<LipShape_v2, float> lipWeight in lipWeightings)
            {
                binLipWeightings[(byte) lipWeight.Key] = lipWeight.Value;
            }

            return binLipWeightings;
        }
                
        // Converts LipWeightings from binary to enumerated.
        private Dictionary<LipShape_v2, float> EnumLipWeightings(Dictionary<byte, float> binLipWeightings)
        {
            Dictionary<LipShape_v2, float> lipWeightings = new Dictionary<LipShape_v2, float>();
                    
            foreach (KeyValuePair<byte, float> lipWeight in binLipWeightings)
            {
                lipWeightings[(LipShape_v2) lipWeight.Key] = lipWeight.Value;
            }

            return lipWeightings;
        }
    }
}