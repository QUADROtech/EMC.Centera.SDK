/******************************************************************************

Copyright © 2006 EMC Corporation. All Rights Reserved
 
This file is part of .NET wrapper for the Centera SDK.

.NET wrapper is free software; you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation version 2.

In addition to the permissions granted in the GNU General Public License
version 2, EMC Corporation gives you unlimited permission to link the compiled
version of this file into combinations with other programs, and to distribute
those combinations without any restriction coming from the use of this file.
(The General Public License restrictions do apply in other respects; for
example, they cover modification of the file, and distribution when not linked
into a combined executable.)

.NET wrapper is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU General Public License version 2 for more
details.

You should have received a copy of the GNU General Public License version 2
along with .NET wrapper; see the file COPYING. If not, write to:

 EMC Corporation 
 Centera Open Source Intiative (COSI) 
 80 South Street
 1/W-1
 Hopkinton, MA 01748 
 USA

******************************************************************************/

using System;
using System.Text;
using System.IO;
using EMC.Centera.SDK;
using EMC.Centera.FPTypes;
using System.Runtime.InteropServices;


namespace WideFilenames
{
    /// <summary>
    /// Demonstrate the use of tge special WideCharacter handling streams
    /// </summary>
    class WideFilenames
    {
        static String defaultCluster = "us1cas1.centera.org?c:\\pea\\us1.pea";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try 
            {
                FPLogger.ConsoleMessage("\nCluster to connect to [" + defaultCluster + "] : ");
                String clusterAddress = System.Console.ReadLine();
                if ("" == clusterAddress) 
                {
                    clusterAddress = defaultCluster;
                }

				UTF8Encoding converter2 = new UTF8Encoding();

                // We are going to create a filename containing Chinese characters

                byte[] inputFileName = {0xE6, 0x98, 0x8E, 0xE5, 0xA4, 0xA9, 0x2E, 0x74, 0x78, 0x74};
                string fileName = System.Text.Encoding.UTF8.GetString(inputFileName);
                File.WriteAllText(fileName, "Test file with wide characters in the file name\n");
              
                FPLogger.ConsoleMessage("\nEnter the threshold to use for Embedded Blobs: ");
                String blobString = System.Console.ReadLine();

                if (blobString == "")
                    blobString = "0";
                int blobThreshold = Int32.Parse(blobString);
                
                if (blobThreshold > 0)
                    FPLogger.ConsoleMessage("\nContent less than " + blobString + " bytes will be embedded in the CDF.");
                else
                    FPLogger.ConsoleMessage("\nContent will never be embedded in the CDF.");

                FPPool.EmbeddedBlobThreshold = blobThreshold;
				FPPool thePool = new FPPool(clusterAddress);

                // Create the clip
                FPClip clipRef = new FPClip(thePool, "UTF8_Example");
				clipRef.SetAttribute("OriginalFilename", fileName);

                FPTag fileTag = clipRef.AddTag("WideFilenameTag");
				fileTag.SetAttribute("filename", fileName);
                
                // Write the content of the file to the Centera 
                FPStream streamRef = new FPWideFilenameStream(fileName);
				fileTag.BlobWrite(streamRef, FPMisc.OPTION_CLIENT_CALCID);
                streamRef.Close();

                long blobSize = fileTag.BlobSize;

                if (blobThreshold > 0 && blobSize < blobThreshold)
                    FPLogger.ConsoleMessage("\nContent was embedded in the CDF as size "
                        + blobSize
                        + " is less than the threshold.");

				fileTag.Close();
				fileTag = clipRef.AddTag("TestTag");

				fileTag.SetAttribute("fileCabinetGuid", "52d9cf57-7261-472a-b6d9-a8cdd30d1d27");
				fileTag.SetAttribute("attr2", "a second attribute");
				fileTag.SetAttribute("anotherattr", "the third attribute");

				/* Or you can write from a memory buffer holding the data
				 * The exampe uses a byte array containing a string
				 */
				UTF8Encoding converter = new UTF8Encoding();
				byte[] rawBuffer = converter.GetBytes("This is a test string to illustrate how to interface to Centera using a buffer stream");
				IntPtr source = Marshal.AllocHGlobal(rawBuffer.Length);
				Marshal.Copy(rawBuffer, 0, source, rawBuffer.Length);
				streamRef = new FPStream(source, rawBuffer.Length, FPStream.StreamDirection.InputToCentera);                				
				fileTag.BlobWrite(streamRef, FPMisc.OPTION_CLIENT_CALCID);
				
				streamRef.Close();
				Marshal.FreeHGlobal(source);
				fileTag.Close();

                fileTag = clipRef.AddTag("TestTag2");
                fileTag.SetAttribute("filename", fileName);

                // We will only store a fragment of the file - "length" characters from position "offset"
                long offset = 5;
                long length = 30;

                streamRef = new FPWideFilenameStream(fileName, offset, length);
                fileTag.BlobWrite(streamRef, FPMisc.OPTION_CLIENT_CALCID);
                streamRef.Close();

                // And this is how we read back at most "length" characters into a file at position "offset"
                // We can also specify that we don't want the created file to be larger than "maxFileSize"
                long maxFileSize = 20;
                streamRef = new FPWideFilenameStream(fileName + ".partial", FileMode.OpenOrCreate, offset, length, maxFileSize);
                fileTag.BlobRead(streamRef);
                streamRef.Close();
                
                String clipID = clipRef.Write();

                /* Write the Clip ID to the output file, "inputFileName.clipID" */
                FPLogger.ConsoleMessage("\nThe C-Clip ID of the content is " + clipID);
                FileStream outFile = new FileStream(fileName + ".clipID", FileMode.Create);
                BinaryWriter outWriter = new BinaryWriter(outFile);
                outWriter.Write(clipID.ToString() + "\n");

                outWriter.Close();
                outFile.Close();
                fileTag.Close();
                clipRef.Close();
                thePool.Close();
            } 
            catch (FPLibraryException e) 
            {
                ErrorInfo err = e.errorInfo;                
				FPLogger.ConsoleMessage("\nException thrown in FP Library: Error " + err.error + " " + err.message);
            }
        }
    }
}

