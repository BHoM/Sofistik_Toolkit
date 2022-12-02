/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Adapter;
using BH.oM.Analytical.Results;
using BH.oM.Base;
using BH.oM.Structure.Requests;
using BH.oM.Structure.Results;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SofistikDataTypes;

namespace BH.Adapter.Sofistik
{
#if DEBUG32BIT || RELEASE32BIT
    unsafe public partial class Sofistik32BitAdapter : BHoMAdapter
#else
    unsafe public partial class SofistikAdapter : BHoMAdapter
#endif
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public IEnumerable<IResult> ReadResults(BarResultRequest request, ActionConfig actionConfig = null)
        {
            List<IResult> results;
            List<int> objectIds = GetObjectIDs(request);
            List<int> loadCases = GetLoadcaseIDs(request);

            switch (request.ResultType)
            {
                case BarResultType.BarForce:
                    results = ExtractBarForce(objectIds, loadCases).ToList();
                    break;
                //case BarResultType.BarStrain:
                //    results = ExtractBarStrain(objectIds, loadCases).ToList();
                //    break;
                //case BarResultType.BarStress:
                //    results = ExtractBarStress(objectIds, loadCases).ToList();
                //    break;
                //case BarResultType.BarDisplacement:
                //    results = ExtractBarDisplacement(objectIds, loadCases).ToList();
                //    break;
                default:
                    Engine.Base.Compute.RecordError($"Result of type {request.ResultType} is not yet supported in the SOFiSTiK_Toolkit.");
                    results = new List<IResult>();
                    break;
            }

            results.Sort();
            return results;
        }

#if DEBUG32BIT || RELEASE32BIT
        /***************************************************/
        /**** private methods                           ****/
        /***************************************************/

        [DllImport("cdb_w31.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_init(
            string name_,
            int initType_
        );

        // sof_cdb_close
        [DllImport("cdb_w31.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sof_cdb_close(
            int index_);

        // sof_cdb_status
        [DllImport("cdb_w31.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_status(
            int index_);

        // sof_cdb_flush
        [DllImport("cdb_w31.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_flush(
            int index_);

        // sof_cdb_flush
        [DllImport("cdb_w31.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_free(
            int kwh_,
            int kwl_);

        // sof_cdb_flush
        [DllImport("cdb_w31.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sof_cdb_kenq_ex(
            int index,
            ref int kwh_,
            ref int kwl_,
            int request_);

        // sof_cdb_get
        [DllImport("cdb_w31.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_get(
            int index_,
            int kwh_,
            int kwl_,
            void* type_,
            ref int recLen_,
            int pos);

#else
        /***************************************************/
        /**** private methods                           ****/
        /***************************************************/

        [DllImport("sof_cdb_w-2022.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_init(
            string name_,
            int initType_
        );

        // sof_cdb_close
        [DllImport("sof_cdb_w-2022.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sof_cdb_close(
            int index_);

        // sof_cdb_status
        [DllImport("sof_cdb_w-2022.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_status(
            int index_);

        // sof_cdb_flush
        [DllImport("sof_cdb_w-2022.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_flush(
            int index_);

        // sof_cdb_flush
        [DllImport("sof_cdb_w-2022.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_free(
            int kwh_,
            int kwl_);

        // sof_cdb_flush
        [DllImport("sof_cdb_w-2022.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sof_cdb_kenq_ex(
            int index,
            ref int kwh_,
            ref int kwl_,
            int request_);

        // sof_cdb_get
        [DllImport("sof_cdb_w-2022.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sof_cdb_get(
            int index_,
            int kwh_,
            int kwl_,
            void* type_,
            ref int recLen_,
            int pos);
#endif
        private IEnumerable<IResult> ExtractBarForce(List<int> ids, List<int> loadcaseIds)
        {
            List<BarForce> barForces = new List<BarForce>();

            int index = 0;
            int status = 0;

            // Define the path of the dlls
#if DEBUG32BIT || RELEASE32BIT
            string directory1 = @"C:\Program Files\SOFiSTiK\2022\SOFiSTiK 2022\interfaces\32bit";

            // Get the path
            string path = Environment.GetEnvironmentVariable("path");

            // Set the new path environment variable + SOFiSTiK dlls path
            path = directory1 + ";" + path;
#else
            string directory1 = @"C:\Program Files\SOFiSTiK\2022\SOFiSTiK 2022\interfaces\64bit";
            string directory2 = @"C:\Program Files\SOFiSTiK\2022\SOFiSTiK 2022";

            // Get the path
            string path = Environment.GetEnvironmentVariable("path");

            // Set the new path environment variable + SOFiSTiK dlls path
            path = directory1 + ";" + directory2 + ";" + path;
#endif

            // Set the path variable (to read the data from CDB)
            System.Environment.SetEnvironmentVariable("path", path);

            // Connect to CDB, int sof_cdb_init  ( char* FileName, int Index);
            index = sof_cdb_init(filePath, 99);
            status = sof_cdb_status(index);

            // data as cs_beam_foc
            cs_beam_foc data;

            // get the length of the structure
            int datalen = System.Runtime.InteropServices.Marshal.SizeOf(typeof(cs_beam_foc));

            List<string> components = new List<string>() { "Id", "Nr", "Pos_x", "LC", "N", "Vy", "Vz", "Mt", "My", "Mz" };

            foreach (int loadcaseId in loadcaseIds)
            {
                while (sof_cdb_get(index, 102, loadcaseId, &data, ref datalen, 1) < 2)
                {
                    int barId = data.m_id;

                    if (ids.Contains(barId))
                    {
                        BarForce barForce = new BarForce(
                        barId,
                        loadcaseId,
                        0,
                        0,
                        data.m_x,
                        2,
                        data.m_n,
                        data.m_vy,
                        data.m_vz,
                        data.m_mt,
                        data.m_my,
                        data.m_mz
                        );

                        barForces.Add(barForce);
                    }
                    // check again for the length
                    datalen = System.Runtime.InteropServices.Marshal.SizeOf(typeof(cs_beam_foc));
                }
                sof_cdb_flush(index);

            }

            // close the CDB
            sof_cdb_close(0);

            return barForces;
        }
    }
}
