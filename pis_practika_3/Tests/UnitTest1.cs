using NUnit.Framework;
using pis_practika_3; 
using System;


namespace Tests
{
    public class Tests
    {
        [TestFixture] 
        public class FuelParserTests
        {

            [Test] 
            public void Parse_ValidInput_ReturnsCorrectObject()
            {
                string input = "¿»-95, 2024.01.01, 50.5, ≈‚Ó-5";

                FuelPrice result = FuelParser.Parse(input);

                Assert.That(result.Type, Is.EqualTo("¿»-95"));          
                Assert.That(result.Cost, Is.EqualTo(50.5));             
                Assert.That(result.QualityGrade, Is.EqualTo("≈‚Ó-5")); 
            }


            [Test]
            public void Parse_InvalidDate_ThrowsFormatException()
            {
                string input = "¿»-95, 2024-01-01, 50.5";

                Assert.Throws<FormatException>(() => FuelParser.Parse(input));
            }


            [Test]
            public void Parse_NegativePrice_ThrowsArgumentException()
            {
                string input = "¿»-95, 2024.01.01, -100";

                Assert.Throws<ArgumentException>(() => FuelParser.Parse(input));
            }
        }
    }
}