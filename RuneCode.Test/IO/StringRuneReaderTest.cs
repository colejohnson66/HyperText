/* =============================================================================
 * File:   StringRuneReaderTest.cs
 * Author: Cole Tobin
 * =============================================================================
 * Purpose:
 *
 * <TODO>
 * =============================================================================
 * Copyright (c) 2021 Cole Tobin
 *
 * This file is part of RuneCode.
 *
 * RuneCode is free software: you can redistribute it and/or modify it under the
 *   terms of the GNU General Public License as published by the Free Software
 *   Foundation, either version 3 of the License, or (at your option) any later
 *   version.
 *
 * RuneCode is distributed in the hope that it will be useful, but WITHOUT ANY
 *   WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 *   FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 *   details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   RuneCode. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneCode.IO;

namespace RuneCode.Test.IO;

[TestClass]
public class StringRuneReaderTest
{
    private const int TEST_STRING_LENGTH = 1024 * 1024;

    [TestMethod]
    public void AsciiTest()
    {
        Random random = new();
        StringBuilder testBuilder = new(TEST_STRING_LENGTH);

        for (int i = 0; i < 16; i++)
            testBuilder.Append((char)random.Next(0x20, 0x7E + 1)); // plus 1 to make it inclusive

        string test = testBuilder.ToString();
        StringRuneReader reader = new(test);
        foreach (char c in test)
            Assert.AreEqual(c, reader.Read().Value.Value);
    }

    [TestMethod]
    public void SurrogateTest()
    {
        Random random = new();
        StringBuilder testBuilder = new(TEST_STRING_LENGTH * 2); // surrogates take two chars
        Rune[] runes = new Rune[TEST_STRING_LENGTH];

        for (int i = 0; i < TEST_STRING_LENGTH; i++)
        {
            while (true)
            {
                int c = random.Next(0x20, 0x10FFFF + 1); // plus 1 to make it inclusive
                if (Rune.IsValid(c)) // ensure the result is a scalar ()
                {
                    Rune r = new(c);
                    testBuilder.Append(r.ToString());
                    runes[i] = r;
                    break;
                }
            }
        }

        string test = testBuilder.ToString();
        StringRuneReader reader = new(test);
        for (int i = 0; i < TEST_STRING_LENGTH; i++)
            Assert.AreEqual(runes[i], reader.Read().Value);
    }
}
