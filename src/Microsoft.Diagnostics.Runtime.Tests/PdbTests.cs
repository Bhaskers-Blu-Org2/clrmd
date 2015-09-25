﻿using Microsoft.Diagnostics.Runtime.Utilities;
using Microsoft.Diagnostics.Runtime.Utilities.Pdb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Runtime.Tests
{
    [TestClass]
    public class PdbTests
    {
        [TestMethod]
        public void PdbGuidAgeTest()
        {
            int pdbAge;
            Guid pdbGuid;
            using (PdbReader pdbReader = new PdbReader(TestTargets.NestedException.Pdb))
            {
                pdbAge = pdbReader.Age;
                pdbGuid = pdbReader.Signature;
            }

            using (PEFile peFile = new PEFile(TestTargets.NestedException.Executable))
            {
                Assert.AreEqual(peFile.PdbInfo.Guid, pdbGuid);
                Assert.AreEqual(peFile.PdbInfo.Revision, pdbAge);
            }
        }

        [TestMethod]
        public void PdbSourceLineTest()
        {
            using (DataTarget dt = TestTargets.NestedException.LoadFullDump())
            {
                ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
                ClrThread thread = runtime.GetMainThread();
                
                HashSet<int> sourceLines = new HashSet<int>();
                using (PdbReader reader = new PdbReader(TestTargets.NestedException.Pdb))
                {
                    var functions = from frame in thread.StackTrace
                                            where frame.Kind != ClrStackFrameType.Runtime
                                            select reader.GetPdbFunctionFor(frame.Method.MetadataToken);

                    foreach (PdbFunction function in functions)
                    {
                        PdbLines sourceFile = function.SequencePoints.Single();

                        foreach (int line in sourceFile.lines.Select(l => l.lineBegin))
                            sourceLines.Add(line);
                    }
                    
                }


                int curr = 0;
                foreach (var line in File.ReadLines(TestTargets.NestedException.Source))
                {
                    curr++;
                    if (line.Contains("/* seq */"))
                        Assert.IsTrue(sourceLines.Contains(curr));
                }
            }
        }
    }
}
