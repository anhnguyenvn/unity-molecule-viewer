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
using System.Collections.Generic;


namespace UMol
{
#if !DISABLE_HIGHLIGHT
    public class UnityMolHighlightManager
    {
        // [DllImport("ComputeHighlightMesh")]
        // public static extern void computeHighlightMesh(Vector3[] modelVertices, int nbVert, int[] modelTri, int nbTri,
        //         float rFac, Vector3[] positions, float[] radii, int N, Vector3[] outVert, int[] outTri);

        /// <summary>
        /// Dictionary of GameObject holding the highlight meshes for each loaded molecule
        /// </summary>
        public Dictionary<UnityMolStructure, GameObject>
            highlightDict = new Dictionary<UnityMolStructure, GameObject>();

        // private bool firstTry = true;
        private bool useCPP;

        private UnityMolSelectionManager selM;

        private Mesh sphereMesh;
        private Vector3[] sphereVertices;
        private int[] sphereTriangles;
        private Vector3[] sphereNormals;

        public float highlightScaleFactor = 1.0f;

        public Material highlightMat;
        Camera mainCam;

        private UnityMolSelection saveSel;


        void getSphereVertices(bool cube = false)
        {
            GameObject tmpSphere = null;
            if ( cube )
            {
                tmpSphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // sphereMesh = tmpSphere.GetComponent<MeshFilter>().sharedMesh;
                sphereMesh = IcoSphereCreator.Create(1, 1.0f);
            }
            else
            {
                sphereMesh = IcoSphereCreator.Create(2, 1.0f);
            }

            sphereTriangles = sphereMesh.triangles;
            sphereVertices = sphereMesh.vertices;
            sphereNormals = sphereMesh.normals;

            mainCam = Camera.main;

            if ( cube )
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(tmpSphere);
#else
            GameObject.Destroy(tmpSphere);
#endif
            }
        }

        public void changeMaterial(Material newMat)
        {
            highlightMat = newMat;
            foreach (GameObject go in highlightDict.Values)
            {
                if ( go != null )
                {
                    go.GetComponent<MeshRenderer>().sharedMaterial = newMat;
                }
            }
        }

        // public void show(bool sh = true) {
        //     foreach (UnityMolStructure s in highlightDict.Keys) {
        //         GameObject go = highlightDict[s];
        //         go.GetComponent<MeshRenderer>().enabled = sh;
        //     }

        public void Clean()
        {
            foreach (GameObject go in highlightDict.Values)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(go);
#else
            GameObject.Destroy(go);
#endif
            }

            highlightDict.Clear();
        }
    }

#endif
}