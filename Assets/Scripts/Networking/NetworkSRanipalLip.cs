//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
//========= Modified for networking with Photon
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace ViveSR
{
    namespace anipal
    {
        namespace Lip
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
                            if (lipShapeTables[table].skinnedMeshRenderer == null)
                            {
                                valid = false;
                                break;
                            }
                            for (int shape = 0; shape < lipShapeTables[table].lipShapes.Length; ++shape)
                            {
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

                [PunRPC]
                public void UpdateLipShapes(Dictionary<byte, float> binLipWeightings)
                {
                    Dictionary<LipShape_v2, float> lipWeightings = EnumLipWeightings(binLipWeightings);
                    
                    foreach (var table in LipShapeTables)
                        RenderModelLipShape(table, lipWeightings);
                }

                private void RenderModelLipShape(LipShapeTable_v2 lipShapeTable, Dictionary<LipShape_v2, float> weighting)
                {
                    // string logString = "";

                    for (int i = 0; i < lipShapeTable.lipShapes.Length; i++)
                    {
                        int targetIndex = (int)lipShapeTable.lipShapes[i];
                        if (targetIndex > (int)LipShape_v2.Max || targetIndex < 0) continue;
                        lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[(LipShape_v2)targetIndex] * 100);

                       // logString += $"{(LipShape_v2)i}: {weighting[(LipShape_v2)targetIndex]}";
                    }

                    // if (!_photonView.IsMine) Debug.LogError(logString);
                }
                
                /// <summary>
                /// Converts LipWeightings to be serializable by Photon.
                /// </summary>
                /// <param name="lipWeightings">LipWeightings to be converted.</param>
                /// <returns>
                /// Serializable Dictionary of LipWeightings with byte keys.
                /// </returns>
                private Dictionary<byte, float> BinaryLipWeightings(Dictionary<LipShape_v2, float> lipWeightings)
                {
                    Dictionary<byte, float> binLipWeightings = new Dictionary<byte, float>();
                    
                    foreach (KeyValuePair<LipShape_v2, float> lipWeight in lipWeightings)
                    {
                        binLipWeightings[(byte) lipWeight.Key] = lipWeight.Value;
                    }

                    return binLipWeightings;
                }
                
                /// <summary>
                /// Converts LipWeightings from binary to enumerated.
                /// </summary>
                /// <param name="lipWeightings">LipWeightings to be converted.</param>
                /// <returns>
                /// Dictionary with enumeration of LipShapes.
                /// </returns>
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
    }
}