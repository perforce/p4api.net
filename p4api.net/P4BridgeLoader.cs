/*******************************************************************************

Copyright (c) 2022, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
	notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
	notice, this list of conditions and the following disclaimer in the
	documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: P4BridgeLoader.cs
 *
 * Author	:
 *
 * Description	: Class containing the logic for loading p4bridge.dll.
 *
 ******************************************************************************/
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Perforce.P4
{
    internal static class P4BridgeLoader
    {
        public const string P4BridgeDll = "p4bridge.dll";

        public static void Load()
        {
            Assembly p4apinet = Assembly.GetExecutingAssembly();
            PortableExecutableKinds peKind;
            ImageFileMachine machine;
            p4apinet.ManifestModule.GetPEKind(out peKind, out machine);

            string p4BridgeRelativePath = P4BridgeDll;

            if (peKind.ToString() == "ILOnly")
            {
                string currentArchSubPath = "x86";

                // Is this a 64 bits process?
                if (IntPtr.Size == 8)
                {
                    currentArchSubPath = "x64";
                }
                p4BridgeRelativePath = Path.Combine(currentArchSubPath, P4BridgeDll);
            }

            LoadLibrary(p4BridgeRelativePath);
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
    }
}
