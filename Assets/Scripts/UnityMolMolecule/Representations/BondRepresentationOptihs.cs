/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2021
        Marc Baaden, 2010-2021
        baaden@smplinux.de
        http://www.baaden.ibpc.fr

        This software is a computer program based on the Unity3D game engine.
        It is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications. More details about UnityMol are provided at
        the following URL: "http://unitymol.sourceforge.net". Parts of this
        source code are heavily inspired from the advice provided on the Unity3D
        forums and the Internet.

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        References :
        If you use this code, please cite the following reference :
        Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
        "Game on, Science - how video game technology may help biologists tackle
        visualization challenges" (2013), PLoS ONE 8(3):e57990.
        doi:10.1371/journal.pone.0057990

        If you use the HyperBalls visualization metaphor, please also cite the
        following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
        B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
        using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
        J. Comput. Chem., 2011, 32, 2924

    Please contact unitymol@gmail.com
    ================================================================================
*/


using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace UMol
{
    public class BondRepresentationOptihs : BondRepresentation
    {
        public List<GameObject> meshesGO;
        public Dictionary<AtomDuo, Int3> coordStickTexture;
        public Dictionary<UnityMolAtom, List<AtomDuo>> atomToDuo;
        public Texture2D[] paramTextures;
        public Dictionary<UnityMolAtom, Color> colorPerAtom;

        public float shrink = 0.4f;
        public float scale = 0.33f;

        public bool withShadow = true;


        public BondRepresentationOptihs(string structName, UnityMolSelection sel)
        {
            colorationType = colorType.atom;

            GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

            representationParent = loadedMolGO.transform.Find(structName);
            
            if ( representationParent == null )
            {
                representationParent = (new GameObject(structName).transform);
                representationParent.parent = loadedMolGO.transform;
                representationParent.localPosition = Vector3.zero;
                representationParent.localRotation = Quaternion.identity;
                representationParent.localScale = Vector3.one;
            }

            GameObject newRep = new GameObject("BondOptiHSRepresentation");
            newRep.transform.parent = representationParent;
            representationTransform = newRep.transform;

            selection = sel;

            DisplayHSMesh(newRep.transform);

            // newRep.transform.position -= offset;

            newRep.transform.localPosition = Vector3.zero;
            newRep.transform.localRotation = Quaternion.identity;
            newRep.transform.localScale = Vector3.one;

            nbBonds = selection.bonds.Count;
        }

        private void DisplayHSMesh(Transform repParent)
        {
            meshesGO = new List<GameObject>();

            int idmesh = 0;
            int cptSticks = 0;
            int currentVertex = 0;
            const int NBPARAM = 14; // Number of parameters in the texture

            // List<KeyValuePair<UnityMolAtom,UnityMolAtom>> allBonds = linearizeBonds();
            long nbSticks = selection.bonds.Length;
            if ( nbSticks == 0 )
                return;

            GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentGO.transform.name = "BondMesh0";
            currentGO.transform.parent = repParent;
            GameObject.Destroy(currentGO.GetComponent<Collider>());

            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh tmpMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            GameObject.Destroy(tmp);

            Vector3[] verticesBase = tmpMesh.vertices;
            int[] trianglesBase = tmpMesh.triangles;

            int VERTICES_IN_CUBE = verticesBase.Length; // 24 vertices by cube
            // Unity limit : number of vertices in a mesh
            int LIMIT_VERTICES_IN_MESH = 65534/4;
            long totalVertices = nbSticks*VERTICES_IN_CUBE;
            int maxSticksinMesh = (int)LIMIT_VERTICES_IN_MESH/VERTICES_IN_CUBE;
            int currentSticksinMesh = (int)Mathf.Min(maxSticksinMesh, nbSticks - (idmesh*maxSticksinMesh));

            // Compute the number of meshes needed to store all the sticks
            int nbMeshesNeeded = Mathf.CeilToInt(totalVertices/(float)LIMIT_VERTICES_IN_MESH) + 1;

            Mesh[] combinedMeshes = new Mesh[nbMeshesNeeded];
            combinedMeshes[idmesh] = new Mesh();
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector2> newUV = new List<Vector2>();
            List<int> newTriangles = new List<int>();

            // Create a texture to store parameters of sticks (11 parameters to store)
            // It is used to fetch parameters for each stick in the shader
            paramTextures = new Texture2D[nbMeshesNeeded];
            paramTextures[idmesh] = new Texture2D(currentSticksinMesh, NBPARAM, TextureFormat.RGBAFloat, false);

            float brightness = 1.0f;
            float attenuation = 0.0f;


            coordStickTexture = new Dictionary<AtomDuo, Int3>();
            atomToDuo = new Dictionary<UnityMolAtom, List<AtomDuo>>();
            colorPerAtom = new Dictionary<UnityMolAtom, Color>();
            AtomDuo key;
            AtomDuo invkey;
            // KeyValuePair<UnityMolAtom, UnityMolAtom> key2;

            var keys = Dbonds.Keys;
            foreach (UnityMolAtom atom1 in keys)
            {
                UnityMolAtom[] bonded = Dbonds[atom1];
                for (int a = 0; a < bonded.Length; a++)
                {
                    UnityMolAtom atom2 = bonded[a];
                    if ( bonded[a] != null )
                    {
                        key = new AtomDuo(atom1, atom2);
                        invkey = new AtomDuo(atom2, atom1);

                        if ( !coordStickTexture.ContainsKey(key) )
                        {
                            //&& !coordStickTexture.TryGetValue(key2, out dummy)) { // Not already done

                            if ( cptSticks == currentSticksinMesh )
                            {
                                RecordMeshAndParameters(combinedMeshes[idmesh], paramTextures[idmesh],
                                    newVertices.ToArray(), newTriangles.ToArray(), newUV.ToArray());

                                combinedMeshes[idmesh].RecalculateBounds();
                                currentGO.GetComponent<MeshFilter>().mesh = combinedMeshes[idmesh];
                                meshesGO.Add(currentGO);

                                AssignMaterial(currentGO, paramTextures[idmesh], brightness,
                                    attenuation, NBPARAM, cptSticks);


                                //Create a new gameObject for the next mesh
                                currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                currentGO.transform.name = "BondMesh" + (idmesh + 1).ToString();
                                currentGO.transform.parent = repParent;
                                GameObject.Destroy(currentGO.GetComponent<Collider>());


                                newVertices.Clear();
                                newUV.Clear();
                                newTriangles.Clear();
                                idmesh++;

                                currentSticksinMesh = (int)Mathf.Max(0,
                                    Mathf.Min(maxSticksinMesh, nbSticks - (idmesh*maxSticksinMesh)));
                                paramTextures[idmesh] = new Texture2D(currentSticksinMesh, NBPARAM,
                                    TextureFormat.RGBAFloat, false);
                                combinedMeshes[idmesh] = new Mesh();
                                currentVertex = 0;
                                cptSticks = 0;
                            }


                            Vector3 posAtom1 = atom1.position;

                            // Store the vertices
                            for (int v = 0; v < verticesBase.Length; v++)
                            {
                                Vector3 vert = verticesBase[v];
                                newVertices.Add(vert +
                                                posAtom1); // Vector3.Scale(vert,new Vector3(aModel.scale/100f,aModel.scale/100f,aModel.scale/100f))+posAtom);
                                // IMPORTANT : Add the id of the atom in each vertex (used to fetch data in the texture)
                                newUV.Add(new Vector2(cptSticks, 0f));
                            }

                            //Store the triangles
                            for (int t = 0; t < trianglesBase.Length; t++)
                                newTriangles.Add(trianglesBase[t] + (cptSticks*VERTICES_IN_CUBE));

                            float visi = 1.0f;

                            StoreParametersInTexture(paramTextures[idmesh], cptSticks, atom1, atom2, Vector4.zero,
                                visi);


                            // //Create Collider for each atom
                            // GameObject colGO = new GameObject("Collider_Atom_"+i);
                            // SphereCollider sc = colGO.AddComponent<SphereCollider>();
                            // sc.radius = radius;
                            // colGO.transform.parent = collidersParent.transform;
                            // collidersGO.Add(colGO);
                            // colGO.transform.position = posAtom;


                            Int3 infoTex;
                            infoTex.x = idmesh;
                            infoTex.y = cptSticks;
                            infoTex.z = 0;
                            coordStickTexture[key] = infoTex;
                            if ( !atomToDuo.ContainsKey(atom1) )
                                atomToDuo[atom1] = new List<AtomDuo>();
                            if ( !atomToDuo.ContainsKey(atom2) )
                                atomToDuo[atom2] = new List<AtomDuo>();
                            atomToDuo[atom1].Add(key);
                            atomToDuo[atom2].Add(key);

                            // Add the opposite bond
                            // infoTex.z = 1;
                            // key = new KeyValuePair<UnityMolAtom, UnityMolAtom>(atom2, atom1);
                            // coordStickTexture[key] = infoTex;

                            cptSticks++;
                            currentVertex += verticesBase.Length;
                        }
                    }
                }
            }

            if ( cptSticks != 0 )
            {
                RecordMeshAndParameters(combinedMeshes[idmesh], paramTextures[idmesh], newVertices.ToArray(),
                    newTriangles.ToArray(), newUV.ToArray());

                combinedMeshes[idmesh].RecalculateBounds();
                currentGO.GetComponent<MeshFilter>().mesh = combinedMeshes[idmesh];
                meshesGO.Add(currentGO);

                AssignMaterial(currentGO, paramTextures[idmesh], brightness, attenuation, NBPARAM, cptSticks);

                // 	//Debug the texture and the mesh
                // 	// UnityEditor.AssetDatabase.CreateAsset(combinedMeshes[idmesh],"Assets/Resources/tmptest/testMesh"+idmesh.ToString()+".asset");
                // 	// UnityEditor.AssetDatabase.CreateAsset(paramTextures[idmesh],"Assets/Resources/tmptest/testTex"+idmesh.ToString()+".asset");
                // 	// UnityEditor.AssetDatabase.SaveAssets();
            }
        }


        void StoreParametersInTexture(Texture2D tex, int bondId, UnityMolAtom umolAtom1, UnityMolAtom umolAtom2,
            Vector4 offset, float visible = 1.0f)
        {
            Vector4 posAtom1vec4 = umolAtom1.PositionVec4 + offset;
            Vector4 posAtom2vec4 = umolAtom2.PositionVec4 + offset;

            Vector4 radiusv4 = Vector4.zero;
            radiusv4.x = umolAtom1.radius;
            tex.SetPixel(bondId, 0, radiusv4); // Radius sphere 1
            radiusv4.x = umolAtom2.radius;
            tex.SetPixel(bondId, 1, radiusv4); // Radius sphere 2

            tex.SetPixel(bondId, 2, umolAtom1.color);
            colorPerAtom[umolAtom1] = umolAtom1.color;
            tex.SetPixel(bondId, 3, umolAtom2.color);
            colorPerAtom[umolAtom2] = umolAtom2.color;
            tex.SetPixel(bondId, 4, posAtom1vec4); // Changing position
            tex.SetPixel(bondId, 5, posAtom2vec4); // Changing position
            tex.SetPixel(bondId, 6, posAtom1vec4); // Base position
            tex.SetPixel(bondId, 7, posAtom2vec4); // Base position
            tex.SetPixel(bondId, 8, Vector4.one); // Atom type matcap id
            tex.SetPixel(bondId, 9, Vector4.one); // Atom type matcap id 2
            tex.SetPixel(bondId, 10, Vector4.one*visible); // Visibility
            tex.SetPixel(bondId, 11, Vector4.one); // Scale for atom 1 in x and atom 2 in y
            tex.SetPixel(bondId, 12, Vector4.zero); // Atom selected
            tex.SetPixel(bondId, 13, Vector4.zero); // Atom selected
        }

        void RecordMeshAndParameters(Mesh mesh, Texture2D tex, Vector3[] vertices, int[] triangles, Vector2[] uv)
        {
            mesh.Clear();
            // Fill the mesh with the arrays
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;

            // Upload the texture to the GPU
            tex.Apply(false); // DO NOT REMOVE !!!!!!!!!!!!!!!!!!
            tex.wrapMode = TextureWrapMode.Clamp; // Mandatory to access the data in the shader
            tex.filterMode = FilterMode.Point; // Texture will be used reading pixels
            tex.anisoLevel = 0;
        }

        void AssignMaterial(GameObject curGO, Texture2D tex, float brightness,
            float attenuation, int NBPARAM, int sticksInMesh)
        {
            Material HSoptiMat = null;

            if ( !withShadow )
            {
                HSoptiMat = new Material(Shader.Find("UMol/Sticks HyperBalls Merged"));
            }
            else
            {
                HSoptiMat = new Material(Shader.Find("UMol/Sticks HyperBalls Shadow Merged"));
            }

            //Set fixed parameters for the shader
            HSoptiMat.SetTexture("_MainTex", tex);
            HSoptiMat.SetTexture("_MatCap", (Texture)Resources.Load("Images/MatCap/daphz05"));
            HSoptiMat.SetFloat("_Brightness", brightness);
            HSoptiMat.SetFloat("_Attenuation", attenuation);
            HSoptiMat.SetFloat("_NBParam", (float)NBPARAM);
            HSoptiMat.SetFloat("_NBSticks", (float)sticksInMesh);
            HSoptiMat.SetFloat("_Shrink", shrink);
            HSoptiMat.SetFloat("_Visibility", 1f);
            HSoptiMat.SetFloat("_Scale", scale);
            HSoptiMat.SetFloat("_EllipseFactor", 1f);
            curGO.GetComponent<Renderer>().sharedMaterial = HSoptiMat;

            if ( withShadow )
            {
                curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
                curGO.GetComponent<Renderer>().receiveShadows = true;
            }
            else
            {
                curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                curGO.GetComponent<Renderer>().receiveShadows = false;
            }
        }

        public static Vector3 computeOffset(AtomTrio t)
        {
            Vector3 v12 = (t.a2.position - t.a1.position).normalized;
            Vector3 v13 = Vector3.zero;
            if ( t.a3 == null )
            {
                v13 = t.a1.position.normalized;
            }
            else
            {
                v13 = (t.a3.position - t.a1.position).normalized;
            }

            float dp = Vector3.Dot(v12, v13);
            if ( 1 - Mathf.Abs(dp) < 1e-6 )
            {
                v13 = new Vector3(1, 0, 0);
                dp = Vector3.Dot(v12, v13);
                if ( 1 - Mathf.Abs(dp) < 1e-6 )
                {
                    v13 = new Vector3(1, 0, 0);
                    dp = Vector3.Dot(v12, v13);
                }
            }

            Vector3 res = (v12*dp) - v13;
            return res.normalized*0.1f;
        }

        public override void Clean()
        {
        }
    }


    public class AtomDuo
    {
        public UnityMolAtom a1;
        public UnityMolAtom a2;

        public AtomDuo(UnityMolAtom atom1, UnityMolAtom atom2)
        {
            a1 = atom1;
            a2 = atom2;
        }

        public static bool operator ==(AtomDuo lhs, AtomDuo rhs)
        {
            if ( ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs) )
            {
                return false;
            }

            if ( ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs) )
            {
                return true;
            }

            return lhs.a1.Equals(rhs.a1) && lhs.a2.Equals(rhs.a2);
        }

        public static bool operator !=(AtomDuo lhs, AtomDuo rhs)
        {
            if ( ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs) )
            {
                return true;
            }

            if ( ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs) )
            {
                return false;
            }

            return !(lhs == rhs);
        }

        public override bool Equals(object o)
        {
            if ( o is AtomDuo )
            {
                AtomDuo duo2 = (AtomDuo)o;
                return a1.Equals(duo2.a1) && a2.Equals(duo2.a2);
            }

            return false;
        }


        // public override int GetHashCode() {
        // 	return lightHashCode;
        // }

        public override int GetHashCode()
        {
            int lhash = a1.serial;
            unchecked
            {
                const int factor = 9176;

                lhash = lhash*factor + a2.serial;
            }

            return lhash;
        }
    }

    public class AtomTrio
    {
        public UnityMolAtom a1;
        public UnityMolAtom a2;
        public UnityMolAtom a3;

        public AtomTrio(UnityMolAtom atom1, UnityMolAtom atom2, UnityMolAtom atom3)
        {
            a1 = atom1;
            a2 = atom2;
            a3 = atom3;
        }

        public static bool operator ==(AtomTrio lhs, AtomTrio rhs)
        {
            if ( ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs) )
            {
                return false;
            }

            if ( ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs) )
            {
                return true;
            }

            return lhs.a1.Equals(rhs.a1) && lhs.a2.Equals(rhs.a2) && lhs.a3.Equals(rhs.a3);
        }

        public static bool operator !=(AtomTrio lhs, AtomTrio rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object o)
        {
            if ( o is AtomTrio )
            {
                AtomTrio trio2 = (AtomTrio)o;
                return a1 == trio2.a1 && a2 == trio2.a2 && a3 == trio2.a3;
            }

            return false;
        }


        public override int GetHashCode()
        {
            int lhash = a1.serial;
            unchecked
            {
                const int factor = 9176;

                lhash = lhash*factor + a2.serial;
                lhash = lhash*factor + a3.serial;
            }

            return lhash;
        }
    }
}